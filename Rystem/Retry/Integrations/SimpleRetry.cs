namespace System
{
    internal class SimpleRetry : IRetryIntegration
    {
        private readonly int MaxAttempts;
        private int Attempts;
        public SimpleRetry(int maxAttempts)
            => this.MaxAttempts = maxAttempts;
        public bool IsRetryable(Exception exception)
        {
            this.Attempts++;
            return this.Attempts < this.MaxAttempts;
        }
    }
}