namespace Rystem.ZConsoleApp.Tester
{
    public interface IKeyValue
    {
        string Storage { get; }
        string Sql { get; }
        string EventHub { get; }
        string ServiceBus { get; }
        string Redis { get; }
    }
}