using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rystem.ConsoleApp.Tester
{
    class Program
    {
        static List<Type> types = new List<Type>();
        static void Main(string[] args)
        {
            string result = WhatDoYouWantToSeeInAction();
            if (result == "exit") return;
            do
            {
                try
                {
                    Console.WriteLine("Insert string to test or whitespace to use default");
                    Console.Write("Insert here: ");
                    string reader = Console.ReadLine();

                    Type type = types[int.Parse(result)];
                    var x = Activator.CreateInstance(type);
                    MethodInfo mi = type.GetMethod("DoWork");
                    if (mi != null)
                    {
                        bool resultX = (bool)mi.Invoke(x, new object[1] { reader });
                        Console.WriteLine("Test: " + resultX);
                    }
                    Console.Write("Press any button to continue");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message);
                    Console.Write("Press any button to continue");
                    Console.ReadLine();
                }
            } while ((result = WhatDoYouWantToSeeInAction()) != "exit");
        }
        static string WhatDoYouWantToSeeInAction()
        {
            int value = 0;
            if (types.Count == 0) types = Assembly.GetExecutingAssembly().GetTypes().ToList().FindAll(ø => ø.GetInterface("ITest") != null);
            foreach (Type t in types)
            {
                Console.WriteLine("For " + t.Namespace.Replace("Rystem.ZConsoleApp.Tester.", "") + "." + t.Name + " use " + value.ToString());
                value++;
            }
            Console.WriteLine("'exit' if you want to close this app.");
            return Console.ReadLine();
        }
    }
}
