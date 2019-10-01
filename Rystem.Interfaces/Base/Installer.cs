using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Interfaces
{
    internal static class Installer
    {
        private static Dictionary<string, Dictionary<string, Dictionary<Installation, InternalConnection>>> Contexts = new Dictionary<string, Dictionary<string, Dictionary<Installation, InternalConnection>>>();
        private static Dictionary<string, InternalConnection> DefaultContexts = new Dictionary<string, InternalConnection>();
        internal static void ConfigureAsDefault(this Type installerType, string connectionString, params string[] names)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return;
            if (!DefaultContexts.ContainsKey(installerType.FullName))
            {
                List<string> namesAsList = names?.ToList();
                if (namesAsList.Count == 0)
                    namesAsList.Add(installerType.Name);
                DefaultContexts.Add(installerType.FullName, new InternalConnection()
                {
                    ConnectionString = connectionString,
                    Names = namesAsList
                });
            }
        }
        internal static void Configure(this Type installerType, Type entity, string connectionString, Installation installation, params string[] names)
        {
            if (!Contexts.ContainsKey(installerType.FullName))
                Contexts.Add(installerType.FullName, new Dictionary<string, Dictionary<Installation, InternalConnection>>());
            if (!Contexts[installerType.FullName].ContainsKey(entity.FullName))
                Contexts[installerType.FullName].Add(entity.FullName, new Dictionary<Installation, InternalConnection>());
            if (!Contexts[installerType.FullName][entity.FullName].ContainsKey(installation))
            {
                List<string> namesAsList = names?.ToList().FindAll(x => !string.IsNullOrWhiteSpace(x));
                if (namesAsList.Count == 0) namesAsList.Add(entity.Name);
                Contexts[installerType.FullName][entity.FullName].Add(installation, new InternalConnection()
                {
                    ConnectionString = connectionString,
                    Names = namesAsList
                });
            }
        }
        internal static InternalConnection GetConfiguration(this Type installerType, Type entity, Installation installation)
        {
            if (Contexts.ContainsKey(installerType.FullName))
            {
                if (Contexts[installerType.FullName].ContainsKey(entity.FullName))
                {
                    if (Contexts[installerType.FullName][entity.FullName].ContainsKey(installation))
                        return Contexts[installerType.FullName][entity.FullName][installation];
                    else
                        throw new NotImplementedException($"Wrong installation type used {installation} instead {string.Join(",", Contexts[installerType.FullName][entity.FullName].Select(x => x.Key))}.");
                }
                else if (entity.BaseType.IsAbstract && Contexts[installerType.FullName].ContainsKey(entity.BaseType.FullName))
                {
                    if (Contexts[installerType.FullName][entity.BaseType.FullName].ContainsKey(installation))
                    {
                        InternalConnection internalConnection = Contexts[installerType.FullName][entity.BaseType.FullName][installation];
                        if (internalConnection.Names.Count <= 1 && internalConnection.Names.FirstOrDefault() == entity.BaseType.Name)
                            return new InternalConnection()
                            {
                                ConnectionString = internalConnection.ConnectionString,
                                Names = new List<string>() { entity.Name }
                            };
                        else
                            return Contexts[installerType.FullName][entity.BaseType.FullName][installation];
                    }
                    else
                        throw new NotImplementedException($"Wrong abstract installation type used {installation} instead {string.Join(",", Contexts[installerType.FullName][entity.BaseType.FullName].Select(x => x.Key))}.");
                }
                else if (DefaultContexts.ContainsKey(installerType.FullName))
                    return new InternalConnection()
                    {
                        ConnectionString = DefaultContexts[installerType.FullName].ConnectionString,
                        Names = new List<string>() { entity.Name }
                    };
                else
                    throw new NotImplementedException($"{entity.FullName} never installed. Please use Install static method in static constructor of your class to set ConnectionString and names of table.");
            }
            else if (DefaultContexts.ContainsKey(installerType.FullName))
                return new InternalConnection()
                {
                    ConnectionString = DefaultContexts[installerType.FullName].ConnectionString,
                    Names = new List<string>() { entity.Name }
                };
            else
                throw new NotImplementedException($"{installerType.FullName} never installed. Please use Install static method in static constructor of your class to set ConnectionString and names of table.");
        }
        internal class InternalConnection
        {
            internal string ConnectionString { get; set; }
            internal List<string> Names { get; set; }
        }
    }
}
