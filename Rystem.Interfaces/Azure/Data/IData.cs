namespace Rystem.Data
{
    public interface IData : IConfigurator
    {
        string Name { get; set; }
        DataProperties Properties { get; set; }
    }
}