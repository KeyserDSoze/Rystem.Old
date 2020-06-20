using System.Threading.Tasks;

namespace System
{
    public static class CatchExtensions
    {
        public static async Task<Exception> TryCatch(this Func<Task> action)
        {
            try
            {
                await action.Invoke();
                return default;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        public static Exception TryCatch(this Action action)
        {
            try
            {
                action.Invoke();
                return default;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}