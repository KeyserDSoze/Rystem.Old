using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Rystem.Interfaces.Utility
{
    public static class Assembler
    {
        public static readonly List<Type> Types = new List<Type>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        static Assembler()
        {
            List<Assembly> allAssemblies = new List<Assembly>();
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (string dll in Directory.GetFiles(path, "*.dll"))
            {
                try
                {
                    allAssemblies.Add(Assembly.LoadFile(dll));
                }
                catch (Exception)
                {
                }
            }
            foreach (Assembly assembly in allAssemblies)
            {
                try
                {
                    Types.AddRange(assembly.GetExportedTypes());
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
