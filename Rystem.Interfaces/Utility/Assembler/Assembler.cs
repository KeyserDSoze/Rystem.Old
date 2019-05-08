using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rystem.Interfaces.Utility
{
    public class Assembler
    {
        public static readonly List<Type> Types = new List<Type>();
        static Assembler()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (!assembly.FullName.ToLower().Contains("system") && !assembly.FullName.ToLower().Contains("microsoft"))
                        Types.AddRange(assembly.GetTypes());
                }
                catch (Exception er)
                {
                    string weee = er.ToString();
                }
            }
        }
    }
}
