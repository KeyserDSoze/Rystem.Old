using Newtonsoft.Json;
using Rystem.Const;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal partial class SmartQueueIntegration<TEntity> : IQueueIntegration<TEntity>
        where TEntity : IQueue
    {
        private readonly string InsertQuery;
        private readonly string IfOnInsert;
        private readonly string ReadQuery;
        private readonly string DeleteQuery;
        private readonly string DeleteOnReadingQuery;
        private readonly string CleanRetentionQuery;
        private readonly bool CheckDuplication;
        private readonly QueueConfiguration QueueConfiguration;
        private SqlConnection Connection() => new SqlConnection(this.QueueConfiguration.ConnectionString);
        internal SmartQueueIntegration(QueueConfiguration property)
        {
            this.QueueConfiguration = property;
            this.CheckDuplication = property.CheckDuplication > 0;
            if (this.CheckDuplication)
            {
                switch (property.CheckDuplication)
                {
                    case QueueDuplication.Path:
                        this.IfOnInsert = $"IF NOT EXISTS (SELECT TOP 1 * FROM SmartQueue_{property.Name} WHERE" + " Path = {0} and Organization = {1}) ";
                        break;
                    case QueueDuplication.Message:
                        this.IfOnInsert = $"IF NOT EXISTS (SELECT TOP 1 * FROM SmartQueue_{property.Name} WHERE" + " Message = '{2}') ";
                        break;
                    case QueueDuplication.PathAndMessage:
                        this.IfOnInsert = $"IF NOT EXISTS (SELECT TOP 1 * FROM SmartQueue_{property.Name} WHERE" + " Path = {0} and Organization = {1} and Message = '{2}') ";
                        break;
                }
            }
            this.InsertQuery = $"INSERT INTO SmartQueue_{property.Name} (Path, Organization, Message, TimeStamp, Ticks) OUTPUT Inserted.Id VALUES (";
            this.ReadQuery = $"Select top {property.NumberOfMessages} Id, Message from SmartQueue_{property.Name} where Ticks <= ";
            this.DeleteQuery = $"Delete from SmartQueue_{property.Name} where Id = ";
            this.DeleteOnReadingQuery = $"Delete from SmartQueue_{property.Name} where Id in (";
            this.CleanRetentionQuery = $"Delete from SmartQueueDeleted_{property.Name} where DATEDIFF(day, ManagedTime,GETUTCDATE()) > {property.Retention}";
            using (SqlConnection connection = Connection())
            {
                connection.Open();
                using (SqlCommand existingCommand = new SqlCommand($"SELECT count(*) FROM sysobjects WHERE name='SmartQueue_{property.Name}' and xtype='U'", connection))
                {
                    int returnCode = (int)existingCommand.ExecuteScalar();
                    if (returnCode == 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append($"IF NOT EXISTS (SELECT TOP 1 * FROM sysobjects WHERE name='SmartQueue_{property.Name}' and xtype='U')");
                        sb.Append($"CREATE TABLE SmartQueue_{property.Name} (");
                        sb.Append("Id bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,");
                        sb.Append("Path int NOT NULL,");
                        sb.Append("Organization int NOT NULL,");
                        sb.Append("Message varchar(max) NOT NULL,");
                        sb.Append("Timestamp datetime NOT NULL,");
                        sb.Append("Ticks bigint NOT NULL);");
                        sb.Append($"IF NOT EXISTS (SELECT TOP 1 * FROM sysobjects WHERE name='SmartQueueDeleted_{property.Name}' and xtype='U')");
                        sb.Append($"CREATE TABLE SmartQueueDeleted_{property.Name} (");
                        sb.Append("Id bigint NOT NULL PRIMARY KEY,");
                        sb.Append("Path int NOT NULL,");
                        sb.Append("Organization int NOT NULL,");
                        sb.Append("Message varchar(max) NOT NULL,");
                        sb.Append("Timestamp datetime NOT NULL,");
                        sb.Append("Ticks bigint NOT NULL,");
                        sb.Append("ManagedTime datetime NOT NULL);");
                        StringBuilder sbNonClusteredIndex = new StringBuilder();
                        sbNonClusteredIndex.Append($"IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'SmartQueue_{property.Name}_Index')");
                        sbNonClusteredIndex.Append($"CREATE NONCLUSTERED INDEX SmartQueue_{property.Name}_Index ON SmartQueue_{property.Name}(Path, Organization, Timestamp, Ticks) WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY];");
                        sbNonClusteredIndex.Append($"IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'SmartQueueDeleted_{property.Name}_Index')");
                        sbNonClusteredIndex.Append($"CREATE NONCLUSTERED INDEX SmartQueueDeleted_{property.Name}_Index ON SmartQueueDeleted_{property.Name}(Path, Organization, Timestamp, Ticks, ManagedTime) WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY];");
                        StringBuilder sbTrigger = new StringBuilder();
                        sbTrigger.Append($"CREATE TRIGGER Trigger_Delete_{property.Name}");
                        sbTrigger.Append($" ON SmartQueue_{property.Name}");
                        sbTrigger.Append($" AFTER DELETE AS INSERT INTO SmartQueueDeleted_{property.Name} (Id, Path, Organization, Message, Timestamp, Ticks, ManagedTime) SELECT d.Id, d.Path, d.Organization, d.Message, d.Timestamp, d.Ticks, GETUTCDATE() FROM Deleted d");
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
        private async Task<T> Retry<T>(SqlConnection connection, string query, Func<SqlCommand, Task<T>> commandQuery)
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();
            int attempt = 0;
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                while (QueueConfiguration.Retry > attempt)
                {
                    try
                    {
                        return await commandQuery.Invoke(command);
                    }
                    catch (SqlException sqlException)
                    {
                        switch (connection.State)
                        {
                            case ConnectionState.Closed:
                            case ConnectionState.Broken:
                                await connection.OpenAsync();
                                break;
                        }
                    }
                    attempt++;
                }
            }
            return default;
        }
        public async Task<bool> DeleteScheduledAsync(long messageId)
        {
            using (SqlConnection connection = Connection())
                return await Retry(connection, $"{DeleteQuery}{messageId}", ExecuteNonQueryAsync) > 0;
        }
        private async Task<long> SendingAsync(IQueue message, int delayInSeconds, int path, int organization)
        {
            DateTime newDatetime = DateTime.UtcNow.AddSeconds(delayInSeconds);
            string messageToSend = JsonConvert.SerializeObject(message, NewtonsoftConst.AutoNameHandling_NullIgnore_JsonSettings).Replace("'", "''");
            StringBuilder sb = new StringBuilder();
            if (this.CheckDuplication)
                sb.Append(string.Format(this.IfOnInsert, path, organization, messageToSend));
            sb.Append(InsertQuery);
            sb.Append($"{path},{organization},'{messageToSend}',");
            sb.Append($"'{newDatetime.ToString("yyyy-MM-ddTHH:mm:ss")}', {newDatetime.Ticks})");
            using (SqlConnection connection = Connection())
                return await Retry(connection, sb.ToString(), ExecuteScalarAsync);

            async Task<long> ExecuteScalarAsync(SqlCommand command)
            {
                object value = await command.ExecuteScalarAsync();
                if (value != null)
                    return Convert.ToInt64(value);
                else
                    return 0;
            }
        }
        private async Task<bool> SendingBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, int path, int organization)
        {
            DateTime newDatetime = DateTime.UtcNow.AddSeconds(delayInSeconds);
            StringBuilder sb = new StringBuilder();
            foreach (IQueue message in messages)
            {
                sb.Append(InsertQuery);
                sb.Append($"{path},'{organization}','{JsonConvert.SerializeObject(message, NewtonsoftConst.AutoNameHandling_NullIgnore_JsonSettings).Replace("'", "''")}',");
                sb.Append($"'{newDatetime.ToString("yyyy-MM-ddTHH:mm:ss")}', {newDatetime.Ticks});");
            }
            using (SqlConnection connection = Connection())
                return await Retry(connection, sb.ToString(), ExecuteNonQueryAsync) > 0;
        }
        public async Task<IEnumerable<TEntity>> Read(int path, int organization)
        {
            StringBuilder query = new StringBuilder();
            query.Append(this.ReadQuery + DateTime.UtcNow.Ticks.ToString());
            if (path > 0)
                query.Append($" and Path = {path}");
            if (organization > 0)
                query.Append($" and Organization = {organization}");
            using (SqlConnection connection = Connection())
            {
                ReadingWrapper readingWrapper = await Retry(connection, query.ToString(), ExecuteReaderAsync);
                if (readingWrapper.ToDeleteIds.Count > 0)
                    await Retry(connection, $"{this.DeleteOnReadingQuery}{string.Join(",", readingWrapper.ToDeleteIds)})", ExecuteNonQueryAsync);
                return readingWrapper.Messages;
            }

            async Task<ReadingWrapper> ExecuteReaderAsync(SqlCommand command)
            {
                ReadingWrapper reading = new ReadingWrapper();
                using (SqlDataReader myReader = await command.ExecuteReaderAsync())
                {
                    while (await myReader.ReadAsync())
                    {
                        reading.Messages.Add(JsonConvert.DeserializeObject<TEntity>(myReader["Message"].ToString(),
                            NewtonsoftConst.AutoNameHandling_NullIgnore_JsonSettings));
                        reading.ToDeleteIds.Add(int.Parse(myReader["Id"].ToString()));
                    }
                    return reading;
                }
            }
            async Task<long> ExecuteNonQueryAsync(SqlCommand command)
                        => await command.ExecuteNonQueryAsync();
        }
        public async Task<bool> CleanAsync()
        {
            using (SqlConnection connection = Connection())
                return await Retry(connection, CleanRetentionQuery, ExecuteNonQueryAsync) > 0;
        }
        private static async Task<long> ExecuteNonQueryAsync(SqlCommand command)
            => await command.ExecuteNonQueryAsync();
    }
    /// <summary>
    /// No business methods
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal partial class SmartQueueIntegration<TEntity> where TEntity : IQueue
    {
        public async Task<bool> SendAsync(IQueue message, int path, int organization)
           => await SendingAsync(message, 0, path, organization) > 0;
        public async Task<bool> SendBatchAsync(IEnumerable<IQueue> messages, int path, int organization)
          => await SendingBatchAsync(messages, 0, path, organization);
        public async Task<long> SendScheduledAsync(IQueue message, int delayInSeconds, int path, int organization)
            => await SendingAsync(message, delayInSeconds, path, organization);
        public async Task<IEnumerable<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, int path, int organization)
        {
            await SendingBatchAsync(messages, delayInSeconds, path, organization);
            return new List<long>();
        }
        private class ReadingWrapper
        {
            public IList<TEntity> Messages { get; set; } = new List<TEntity>();
            public IList<long> ToDeleteIds { get; set; } = new List<long>();
        }
    }
}
