namespace Rystem.Azure.Queue
{
    /// <summary>
    /// Configuration of your queue
    /// </summary>
    public class QueueConfiguration : IRystemConfiguration
    {
        /// <summary>
        /// ServiceBus, QueueStorage and EventHub use their own connection string. For SmartQueue use a sql connection.
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Name of your Queue
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Choose what you want to implement
        /// </summary>
        public QueueType Type { get; set; }
        /// <summary>
        /// Only available for SmartQueue. Doesn't allow to have a duplicated message.
        /// </summary>
        public QueueDuplication CheckDuplication { get; set; }
        /// <summary>
        /// Only available for SmartQueue. Number of messages read at every request. Default value is 100.
        /// </summary>
        public int NumberOfMessages { get; set; } = 100;
    }
}
