using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester
{
    internal class KeyValue : IKeyValue
    {
        public string Storage => "";
        public string Sql => "";
        public string EventHub => "";
        public string ServiceBus => "";
        public string Redis => "";
        public static KeyValue Instance { get; } = new KeyValue();
        public KeyValue() { }
    }
}