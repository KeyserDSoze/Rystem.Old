using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Rystem.UnitTest
{
    public class UnitTestMetrics
    {
        public const string UnitTestMessageException = "UnitTestMetrics";
        public static UnitTestMetrics DefaultOk { get; } = new UnitTestMetrics(true);
        public static UnitTestMetrics DefaultNotOk { get; } = new UnitTestMetrics(false);
        public int ThreadId { get; }
        public CommandRequest Command { get; }
        public UnitTestMetrics(int threadId, CommandRequest command)
        {
            this.ThreadId = threadId;
            this.Command = command;
        }
        private UnitTestMetrics(bool isOk)
            => this.Add(isOk, null, "Default", string.Empty, string.Empty, 0);
        public DateTime Start { get; } = DateTime.UtcNow;
        public DateTime End { get; private set; }
        public List<UnitTestEvent> UnitTestEvents { get; } = new List<UnitTestEvent>();
        private UnitTestMetrics Add(bool isOk, object value, string message, string callingMethod, string callingFilePath, int callingFileLineNumber)
        {
            this.End = DateTime.UtcNow;
            this.UnitTestEvents.Add(new UnitTestEvent()
            {
                IsCorrect = isOk,
                Message = message,
                Timestamp = this.End,
                TimeFromStart = this.End.Subtract(this.Start),
                TaskId = this.ThreadId,
                CallingFileLineNumber = callingFileLineNumber,
                CallingFilePath = callingFilePath,
                CallingMethod = callingMethod,
                Value = value
            });
            return this;
        }
        public UnitTestMetrics AddOk(object value = null, string message = "",
            [CallerMemberName] string callingMethod = "",
            [CallerFilePath] string callingFilePath = "",
            [CallerLineNumber] int callingFileLineNumber = 0)
            => this.Add(true, value, message, callingMethod, callingFilePath, callingFileLineNumber);
        public UnitTestMetrics AddNotOk(object value = null, string message = "",
            [CallerMemberName] string callingMethod = "",
            [CallerFilePath] string callingFilePath = "",
            [CallerLineNumber] int callingFileLineNumber = 0)
            => this.Add(false, value, message, callingMethod, callingFilePath, callingFileLineNumber);
        public UnitTestMetrics Check(bool isOk, object value = null, string message = "",
            [CallerMemberName] string callingMethod = "",
            [CallerFilePath] string callingFilePath = "",
            [CallerLineNumber] int callingFileLineNumber = 0)
            => this.Add(isOk, value, message, callingMethod, callingFilePath, callingFileLineNumber);
        public UnitTestMetrics CheckIfNotOkExit(bool isNotOk, object value = null, string message = "",
            [CallerMemberName] string callingMethod = "",
            [CallerFilePath] string callingFilePath = "",
            [CallerLineNumber] int callingFileLineNumber = 0)
        {
            this.Add(!isNotOk, value, message, callingMethod, callingFilePath, callingFileLineNumber);
            if (isNotOk)
                throw new Exception(UnitTestMessageException);
            return this;
        }
        public UnitTestMetrics CheckIfOkExit(bool isOk, object value = null, string message = "",
            [CallerMemberName] string callingMethod = "",
            [CallerFilePath] string callingFilePath = "",
            [CallerLineNumber] int callingFileLineNumber = 0)
            => this.CheckIfNotOkExit(!isOk, value, message, callingMethod, callingFilePath, callingFileLineNumber);
    }
}
