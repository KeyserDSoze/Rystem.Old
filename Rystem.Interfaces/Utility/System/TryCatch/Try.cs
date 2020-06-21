using System.Threading.Tasks;

namespace System
{
    public static class Try
    {
        public static async Task<CatchResponse> CatchAsync(Func<Task> action, Func<Exception, Task> catchAction = null)
            => await action.TryCatchAsync(catchAction).NoContext();
        public static async Task<CatchResponse<T>> CatchAsync<T>(Func<Task<T>> action, Func<Exception, Task> catchAction = null)
            => await action.TryCatchAsync(catchAction).NoContext();
        public static CatchResponse Catch(Action action, Action<Exception> catchAction = null)
            => action.TryCatch(catchAction);
        public static CatchResponse<T> Catch<T>(Func<T> action, Action<Exception> catchAction = null)
            => action.TryCatch(catchAction);
    }
}