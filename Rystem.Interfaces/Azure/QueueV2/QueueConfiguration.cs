namespace Rystem.Azure.Queue
{
    public class QueueConfiguration : IRystemConfiguration
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public QueueType Type { get; set; }
        public QueueDuplication CheckDuplication { get; set; }
    }
}
