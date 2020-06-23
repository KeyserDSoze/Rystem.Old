using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Rystem.UnitTest
{
    public class UnitTestEvent
    {
        public bool IsCorrect { get; internal set; }
        public string Message { get; internal set; }
        public DateTime Timestamp { get; internal set; }
        public TimeSpan TimeFromStart { get; internal set; }
        public int TaskId { get; internal set; }
        public string CallingMethod { get; internal set; }
        public string CallingFilePath { get; internal set; }
        public int CallingFileLineNumber { get; internal set; }
        public object Value { get; internal set; }
        public override string ToString() => $"{TaskId} => {TimeFromStart} | {Timestamp} | {Message} | Value:{Value} | LineNumber:{CallingFileLineNumber}";
    }
}