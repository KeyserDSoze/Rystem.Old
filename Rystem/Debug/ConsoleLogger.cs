using Microsoft.Extensions.Logging;
using System;

namespace Rystem.UnitTest
{
    public class ConsoleLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.ResetColor();
            switch (logLevel)
            {
                default:
                case LogLevel.None:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;
                case LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
            }
            Console.WriteLine($"({logLevel}) - {exception?.ToString() ?? state.ToString()}");
            Console.ResetColor();
        }
    }
}
