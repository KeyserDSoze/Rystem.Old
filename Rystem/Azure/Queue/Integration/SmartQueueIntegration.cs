using Newtonsoft.Json;
using Rystem.Const;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.Queue
{
    internal partial class SmartQueueIntegration<TEntity> : IQueueIntegration<TEntity>
    {
        private readonly string InsertQuery;
        private readonly string IfOnInsert;
        private readonly string ReadQuery;
        private readonly string DeleteQuery;
        private readonly string DeleteOnReadingQuery;
        private readonly string CleanRetentionQuery;
        private readonly bool CheckDuplication;
        private readonly QueueConfiguration QueueConfiguration;
        private SqlConnection NewConnection() => new SqlConnection(this.QueueConfiguration.ConnectionString);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "It's built-in query. Noone can inject command. Only in insert I add the parameters pattern check.")]
        internal SmartQueueIntegration(QueueConfiguration property)
        {
            this.QueueConfiguration = property;
            this.CheckDuplication = property.CheckDuplication > 0;
            if (this.CheckDuplication)
            {
                switch (property.CheckDuplication)
                {
                    case QueueDuplication.Path:
                        this.IfOnInsert = $"IF NOT EXISTS (SELECT TOP 1 * FROM SmartQueue_{property.Name} WHERE" + " Path = @Path and Organization = @Organization) ";
                        break;
                    case QueueDuplication.Message:
                        this.IfOnInsert = $"IF NOT EXISTS (SELECT TOP 1 * FROM SmartQueue_{property.Name} WHERE" + " Message = @Message) ";
                        break;
                    case QueueDuplication.PathAndMessage:
                        this.IfOnInsert = $"IF NOT EXISTS (SELECT TOP 1 * FROM SmartQueue_{property.Name} WHERE" + " Path = @Path and Organization = @Organization and Message = @Message) ";
                        break;
                }
            }
            this.InsertQuery = $"INSERT INTO SmartQueue_{property.Name} (Path, Organization, Message, TimeStamp, Ticks) OUTPUT Inserted.Id VALUES (@Path, @Organization, @Message, @TimeStamp, @Ticks)";
            this.ReadQuery = $"Select top {property.NumberOfMessages} Id, Message from SmartQueue_{property.Name} where Ticks <= ";
            this.DeleteQuery = $"Delete from SmartQueue_{property.Name} where Id = ";
            this.DeleteOnReadingQuery = $"Delete from SmartQueue_{property.Name} where Id in (";
            this.CleanRetentionQuery = $"Delete from SmartQueueDeleted_{property.Name} where DATEDIFF(day, ManagedTime, GETUTCDATE()) > {property.Retention}";
            ThreadPool.UnsafeQueueUserWorkItem(this.CreateIfNotExists, new object());
        }
        private void CreateIfNotExists(object state)
        {
            using (SqlConnection connection = NewConnection())
            {
                connection.Open();
                using (SqlCommand existingCommand = new SqlCommand($"SELECT count(*) FROM sysobjects WHERE name='SmartQueue_{this.QueueConfiguration.Name}' and xtype='U'", connection))
                {
                    int returnCode = (int)existingCommand.ExecuteScalar();
                    if (returnCode == 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append($"IF NOT EXISTS (SELECT TOP 1 * FROM sysobjects WHERE name='SmartQueue_{this.QueueConfiguration.Name}' and xtype='U')");
                        sb.Append($"CREATE TABLE SmartQueue_{this.QueueConfiguration.Name} (");
                        sb.Append("Id bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,");
                        sb.Append("Path int NOT NULL,");
                        sb.Append("Organization int NOT NULL,");
                        sb.Append("Message varchar(max) NOT NULL,");
                        sb.Append("Timestamp datetime NOT NULL,");
                        sb.Append("Ticks bigint NOT NULL);");
                        sb.Append($"IF NOT EXISTS (SELECT TOP 1 * FROM sysobjects WHERE name='SmartQueueDeleted_{this.QueueConfiguration.Name}' and xtype='U')");
                        sb.Append($"CREATE TABLE SmartQueueDeleted_{this.QueueConfiguration.Name} (");
                        sb.Append("Id bigint NOT NULL PRIMARY KEY,");
                        sb.Append("Path int NOT NULL,");
                        sb.Append("Organization int NOT NULL,");
                        sb.Append("Message varchar(max) NOT NULL,");
                        sb.Append("Timestamp datetime NOT NULL,");
                        sb.Append("Ticks bigint NOT NULL,");
                        sb.Append("ManagedTime datetime NOT NULL);");
                        StringBuilder sbNonClusteredIndex = new StringBuilder();
                        sbNonClusteredIndex.Append($"IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'SmartQueue_{this.QueueConfiguration.Name}_Index')");
                        sbNonClusteredIndex.Append($"CREATE NONCLUSTERED INDEX SmartQueue_{this.QueueConfiguration.Name}_Index ON SmartQueue_{this.QueueConfiguration.Name}(Path, Organization, Timestamp, Ticks) WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY];");
                        sbNonClusteredIndex.Append($"IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'SmartQueueDeleted_{this.QueueConfiguration.Name}_Index')");
                        sbNonClusteredIndex.Append($"CREATE NONCLUSTERED INDEX SmartQueueDeleted_{this.QueueConfiguration.Name}_Index ON SmartQueueDeleted_{this.QueueConfiguration.Name}(Path, Organization, Timestamp, Ticks, ManagedTime) WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY];");
                        StringBuilder sbTrigger = new StringBuilder();
                        sbTrigger.Append($"CREATE TRIGGER Trigger_Delete_{this.QueueConfiguration.Name}");
                        sbTrigger.Append($" ON SmartQueue_{this.QueueConfiguration.Name}");
                        sbTrigger.Append($" AFTER DELETE AS INSERT INTO SmartQueueDeleted_{this.QueueConfiguration.Name} (Id, Path, Organization, Message, Timestamp, Ticks, ManagedTime) SELECT d.Id, d.Path, d.Organization, d.Message, d.Timestamp, d.Ticks, GETUTCDATE() FROM Deleted d");
                        using (SqlCommand command = new SqlCommand(sb.ToString(), connection))
                            command.ExecuteNonQuery();
                        using (SqlCommand commandForNonClusteredIndex = new SqlCommand(sbNonClusteredIndex.ToString(), connection))
                            commandForNonClusteredIndex.ExecuteNonQuery();
                        using (SqlCommand commandForTrigger = new SqlCommand(sbTrigger.ToString(), connection))
                            commandForTrigger.ExecuteNonQuery();
                    }
                }
            }
        }

        private static readonly SqlException SqlExceptionDefault = default;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "It's built-in query. Noone can inject command")]
        private async Task<T> RetryAsync<T>(SqlConnection connection, string query, List<SqlParameter> parameters, Func<SqlCommand, Task<T>> commandQuery)
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync().NoContext();
            int attempt = 0;
            SqlException sqlException = SqlExceptionDefault;
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                foreach (SqlParameter sqlParameter in parameters)
                    command.Parameters.Add(sqlParameter);
                while (QueueConfiguration.Retry > attempt)
                {
                    try
                    {
                        return await commandQuery.Invoke(command).NoContext();
                    }
                    catch (SqlException ex)
                    {
                        await Task.Delay(100).NoContext();
                        switch (connection.State)
                        {
                            case ConnectionState.Closed:
                            case ConnectionState.Broken:
                            case ConnectionState.Connecting:
                                await connection.OpenAsync().NoContext();
                                break;
                        }
                        sqlException = ex;
                    }
                    attempt++;
                }
            }
            if (attempt >= QueueConfiguration.Retry)
                throw sqlException;
            return default;
        }
        private static readonly List<SqlParameter> EmptyParameters = new List<SqlParameter>();
        public async Task<bool> DeleteScheduledAsync(long messageId)
        {
            using SqlConnection connection = NewConnection();
            return await RetryAsync(connection, $"{DeleteQuery}{messageId}", EmptyParameters, ExecuteNonQueryAsync).NoContext() > 0;
        }
        private async Task<long> SendingAsync(SqlConnection connection, TEntity message, int delayInSeconds, int path, int organization)
        {
            DateTime newDatetime = DateTime.UtcNow.AddSeconds(delayInSeconds);
            string messageToSend = GetNormalizedJson(message);
            string query = this.CheckDuplication ? this.IfOnInsert + this.InsertQuery : this.InsertQuery;
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@Path", path));
            parameters.Add(new SqlParameter("@Organization", organization));
            parameters.Add(new SqlParameter("@Message", messageToSend));
            parameters.Add(new SqlParameter("@TimeStamp", newDatetime));
            parameters.Add(new SqlParameter("@Ticks", newDatetime.Ticks));
            return await RetryAsync(connection, query, parameters, ExecuteScalarAsync).NoContext();

            async Task<long> ExecuteScalarAsync(SqlCommand command)
            {
                object value = await command.ExecuteScalarAsync().NoContext();
                if (value != null)
                    return Convert.ToInt64(value);
                else
                    return 0;
            }
        }
        private async Task<QueueResult> SendingBatchAsync(IEnumerable<TEntity> messages, int delayInSeconds, int path, int organization)
        {
            QueueResult queueResult = new QueueResult()
            {
                IsOk = true,
                IdMessages = new List<long>()
            };
            using SqlConnection connection = NewConnection();
            foreach (TEntity message in messages)
            {
                long id = await SendingAsync(connection, message, delayInSeconds, path, organization).NoContext();
                if (id == 0)
                    queueResult.IsOk = false;
                else
                    queueResult.IdMessages.Add(id);
            }
            return queueResult;
        }
        public async Task<IEnumerable<TEntity>> ReadAsync(int path, int organization)
        {
            StringBuilder query = new StringBuilder();
            query.Append($"{this.ReadQuery}{DateTime.UtcNow.Ticks}");
            if (path > 0)
                query.Append($" and Path = {path}");
            if (organization > 0)
                query.Append($" and Organization = {organization}");
            using (SqlConnection connection = NewConnection())
            {
                ReadingWrapper readingWrapper = await RetryAsync(connection, query.ToString(), EmptyParameters, ExecuteReaderAsync).NoContext();
                if (readingWrapper.ToDeleteIds.Count > 0)
                    await RetryAsync(connection, $"{this.DeleteOnReadingQuery}{string.Join(",", readingWrapper.ToDeleteIds)})", EmptyParameters, ExecuteNonQueryAsync).NoContext();
                return readingWrapper.Messages;
            }

            async Task<ReadingWrapper> ExecuteReaderAsync(SqlCommand command)
            {
                ReadingWrapper reading = new ReadingWrapper();
                using SqlDataReader myReader = await command.ExecuteReaderAsync().NoContext();
                while (await myReader.ReadAsync().NoContext())
                {
                    reading.Messages.Add(myReader["Message"].ToString().ToMessage<TEntity>());
                    reading.ToDeleteIds.Add(int.Parse(myReader["Id"].ToString()));
                }
                return reading;
            }
            async Task<long> ExecuteNonQueryAsync(SqlCommand command)
                        => await command.ExecuteNonQueryAsync().NoContext();
        }
        public async Task<bool> CleanAsync()
        {
            using SqlConnection connection = NewConnection();
            return await RetryAsync(connection, CleanRetentionQuery, EmptyParameters, ExecuteNonQueryAsync).NoContext() > 0;
        }
        private static async Task<long> ExecuteNonQueryAsync(SqlCommand command)
            => await command.ExecuteNonQueryAsync().NoContext();
    }

    // No business methods
    internal partial class SmartQueueIntegration<TEntity>
    {
        public async Task<bool> SendAsync(TEntity message, int path, int organization)
        {
            using SqlConnection connection = NewConnection();
            return await SendingAsync(connection, message, 0, path, organization).NoContext() > 0;
        }
        public async Task<bool> SendBatchAsync(IEnumerable<TEntity> messages, int path, int organization)
            => (await SendingBatchAsync(messages, 0, path, organization).NoContext()).IsOk;
        public async Task<long> SendScheduledAsync(TEntity message, int delayInSeconds, int path, int organization)
        {
            using SqlConnection connection = NewConnection();
            return await SendingAsync(connection, message, delayInSeconds, path, organization).NoContext();
        }
        public async Task<IEnumerable<long>> SendScheduledBatchAsync(IEnumerable<TEntity> messages, int delayInSeconds, int path, int organization)
            => (await SendingBatchAsync(messages, delayInSeconds, path, organization).NoContext()).IdMessages;
        private const string ToReplaceOnQuery = "'";
        private const string ReplaceWithOnQuery = "''";
        private string GetNormalizedJson(TEntity message)
            => message.ToDefaultJson().Replace(ToReplaceOnQuery, ReplaceWithOnQuery);
        private class ReadingWrapper
        {
            public IList<TEntity> Messages { get; set; } = new List<TEntity>();
            public IList<long> ToDeleteIds { get; set; } = new List<long>();
        }
        private class QueueResult
        {
            public bool IsOk { get; set; }
            public IList<long> IdMessages { get; set; }
        }
    }
}