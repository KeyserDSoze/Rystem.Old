using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rystem.ZConsoleApp.Tester
{
    public class KeyManager
    {
        public static IKeyValue Instance { get; private set; } = new KeyValue();
        public static void SetNewManager(IKeyValue keyValue) => Instance = keyValue;
    }
}