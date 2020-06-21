using System.Threading.Tasks;

namespace System
{
    public static class CatchExtensions
    {
        public static async Task<CatchResponse> TryCatchAsync(this Func<Task> action, Func<Exception, Task> catchAction = null)
        {
            try
            {
                await action.Invoke().NoContext();
                return CatchResponse.Empty;
            }
            catch (Exception ex)
            {
                if (catchAction != null)
                    await catchAction.Invoke(ex).NoContext();
                return new CatchResponse(ex);
            }
        }
        public static async Task<CatchResponse<T>> TryCatchAsync<T>(this Func<Task<T>> action, Func<Exception, Task> catchAction = null)
        {
            try
            {
                return new CatchResponse<T>(await action.Invoke().NoContext());
            }
            catch (Exception ex)
            {
                if (catchAction != null)
                    await catchAction.Invoke(ex).NoContext();
                return new CatchResponse<T>(ex);
            }
        }
        public static CatchResponse TryCatch(this Action action, Action<Exception> catchAction = null)
        {
            try
            {
                action.Invoke();
                return CatchResponse.Empty;
            }
            catch (Exception ex)
            {
                if (catchAction != null)
                    catchAction.Invoke(ex);
                return new CatchResponse(ex);
            }
        }
        public static CatchResponse<T> TryCatch<T>(this Func<T> action, Action<Exception> catchAction = null)
        {
            try
            {
                return new CatchResponse<T>(action.Invoke());
            }
            catch (Exception ex)
            {
                if (catchAction != null)
                    catchAction.Invoke(ex);
                return new CatchResponse<T>(ex);
            }
        }
    }
}