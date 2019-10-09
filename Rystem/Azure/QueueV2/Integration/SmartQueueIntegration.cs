using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal class SmartQueueIntegration<TEntity> : IQueueIntegration<TEntity>
        where TEntity : IQueue
    {
        private string ConnectionString;
        private string InsertQuery;
        private string IfOnInsert;
        private string ReadQuery;
        private string DeleteQuery;
        private string DeleteOnReadingQuery;
        private bool CheckDuplication;
        private SqlConnection Connection() => new SqlConnection(ConnectionString);
        internal SmartQueueIntegration(QueueConfiguration property)
        {
            this.ConnectionString = property.ConnectionString;
            this.CheckDuplication = property.CheckDuplication;
            if (this.CheckDuplication)
                this.IfOnInsert = $"IF NOT EXISTS (SELECT * FROM SmartQueue_{property.Name} WHERE" + " Path = {0} and Organization = {1}) ";
            this.InsertQuery = $"INSERT INTO SmartQueue_{property.Name} (Path, Organization, Message, TimeStamp, Ticks) OUTPUT Inserted.Id VALUES (";
            this.ReadQuery = $"Select top 100 Id, Message from SmartQueue_{property.Name} where Ticks <= ";
            this.DeleteQuery = $"Delete from SmartQueue_{property.Name} where Id = ";
            this.DeleteOnReadingQuery = $"Delete from SmartQueue_{property.Name} where Id in (";
            StringBuilder sb = new StringBuilder();
            sb.Append($"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SmartQueue_{property.Name}' and xtype='U')");
            sb.Append($"CREATE TABLE SmartQueue_{property.Name} (");
            sb.Append("Id bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,");
            sb.Append("Path int NOT NULL,");
            sb.Append("Organization int NOT NULL,");
            sb.Append("Message varchar(max) NOT NULL,");
            sb.Append("Timestamp datetime NOT NULL,");
            sb.Append("Ticks bigint NOT NULL);");
            sb.Append($"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SmartQueueDeleted_{property.Name}' and xtype='U')");
            sb.Append($"CREATE TABLE SmartQueueDeleted_{property.Name} (");
            sb.Append("Id bigint NOT NULL PRIMARY KEY,");
            sb.Append("Path int NOT NULL,");
            sb.Append("Organization int NOT NULL,");
            sb.Append("Message varchar(max) NOT NULL,");
            sb.Append("Timestamp datetime NOT NULL,");
            sb.Append("Ticks bigint NOT NULL,");
            sb.Append("ManagedTime datetime NOT NULL);");
            StringBuilder sbTrigger = new StringBuilder();
            sbTrigger.Append($"CREATE TRIGGER Trigger_Delete_{property.Name}");
            sbTrigger.Append($" ON SmartQueue_{property.Name}");
            sbTrigger.Append($" AFTER DELETE AS INSERT INTO SmartQueueDeleted_{property.Name} (Id, Path, Organization, Message, Timestamp, Ticks, ManagedTime) SELECT d.Id, d.Path, d.Organization, d.Message, d.Timestamp, d.Ticks, GETUTCDATE() FROM Deleted d");
            using (SqlConnection connection = Connection())
            {
                connection.Open();
                using (SqlCommand existingCommand = new SqlCommand($"SELECT count(*) FROM sysobjects WHERE name='SmartQueue_{property.Name}' and xtype='U'", connection))
                {
                    int returnCode = (int)existingCommand.ExecuteScalar();
                    if (returnCode == 0)
                    {
                        using (SqlCommand command = new SqlCommand(sb.ToString(), connection))
                            command.ExecuteNonQuery();
                        using (SqlCommand commandForTrigger = new SqlCommand(sbTrigger.ToString(), connection))
                            commandForTrigger.ExecuteNonQuery();
                    }
                }
            }
        }
        public async Task<bool> DeleteScheduledAsync(long messageId)
        {
            using (SqlConnection connection = Connection())
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(DeleteQuery + messageId.ToString(), connection))
                    await command.ExecuteNonQueryAsync();
            }
            return true;
        }

        public async Task<bool> SendAsync(IQueue message, int path, int organization)
            => await SendingAsync(message, 0, path, organization) > 0;
        private async Task<long> SendingAsync(IQueue message, int delayInSeconds, int path, int organization)
        {
            DateTime newDatetime = DateTime.UtcNow.AddSeconds(delayInSeconds);
            StringBuilder sb = new StringBuilder();
            if (this.CheckDuplication)
                sb.Append(string.Format(this.IfOnInsert, path, organization));
            sb.Append(InsertQuery);
            sb.Append($"{path},{organization},'{JsonConvert.SerializeObject(message, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto }).Replace("'", "''")}',");
            sb.Append($"'{newDatetime.ToString("yyyy-MM-ddTHH:mm:ss")}', {newDatetime.Ticks})");
            using (SqlConnection connection = Connection())
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(sb.ToString(), connection))
                {
                    object value = await command.ExecuteScalarAsync();
                    if (value != null)
                        return Convert.ToInt64(value);
                    else
                        return 0;
                }
            }
        }
        private async Task<bool> SendingBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, int path, int organization)
        {
            DateTime newDatetime = DateTime.UtcNow.AddSeconds(delayInSeconds);
            StringBuilder sb = new StringBuilder();
            foreach (IQueue message in messages)
            {
                sb.Append(InsertQuery);
                sb.Append($"{path},'{organization}','{JsonConvert.SerializeObject(message, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto }).Replace("'", "''")}',");
                sb.Append($"'{newDatetime.ToString("yyyy-MM-ddTHH:mm:ss")}', {newDatetime.Ticks});");
            }
            using (SqlConnection connection = Connection())
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(sb.ToString(), connection))
                    await command.ExecuteNonQueryAsync();
            }
            return true;
        }
        public async Task<bool> SendBatchAsync(IEnumerable<IQueue> messages, int path, int organization)
            => await SendingBatchAsync(messages, 0, path, organization);

        public async Task<long> SendScheduledAsync(IQueue message, int delayInSeconds, int path, int organization)
            => await SendingAsync(message, delayInSeconds, path, organization);

        public async Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, int path, int organization)
        {
            await SendingBatchAsync(messages, delayInSeconds, path, organization);
            return new List<long>();
        }

        public async Task<IList<TEntity>> Read(int path, int organization)
        {
            IList<TEntity> messages = new List<TEntity>();
            StringBuilder query = new StringBuilder();
            query.Append(this.ReadQuery + DateTime.UtcNow.Ticks.ToString());
            if (path > 0)
                query.Append($" and Path = {path}");
            if (organization > 0)
                query.Append($" and Organization = {organization}");
            using (SqlConnection connection = Connection())
            {
                IList<int> idToDelete = new List<int>();
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query.ToString(), connection))
                {
                    using (SqlDataReader myReader = await command.ExecuteReaderAsync())
                    {
                        while (await myReader.ReadAsync())
                        {
                            messages.Add(JsonConvert.DeserializeObject<TEntity>(myReader["Message"].ToString(),
                                new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto }));
                            idToDelete.Add(int.Parse(myReader["Id"].ToString()));
                        }
                    }
                }
                if (idToDelete.Count > 0)
                {
                    string queryForDeleting = $"{this.DeleteOnReadingQuery}{string.Join(",", idToDelete)})";
                    using (SqlCommand command = new SqlCommand(queryForDeleting, connection))
                        await command.ExecuteNonQueryAsync();
                }
            }
            return messages;
        }
    }
}
