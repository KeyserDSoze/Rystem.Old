namespace System
{
    public class CatchResponse
    {
        public Exception Exception { get; }
        public bool InException => this.Exception != null;
        public CatchResponse(Exception exception)
            => this.Exception = exception;
        private CatchResponse() { }
        public static CatchResponse Empty { get; } = new CatchResponse();
    }
    public class CatchResponse<T> : CatchResponse
    {
        public T Result { get; }
        public CatchResponse(Exception exception) : base(exception) { }
        public CatchResponse(T result) : base(default)
            => this.Result = result;
    }
}