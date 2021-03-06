namespace System
{
    internal interface IRetryIntegration
    {
        bool IsRetryable(Exception exception);
        int Attempts { get; }
    }
}