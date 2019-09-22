using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem
{
    interface IRystemConfiguration
    {
        string Name { get; set; }
    }
    internal static class Installer<TConfiguration>
        where TConfiguration : IRystemConfiguration
    {
        internal static TConfiguration DefaultConfiguration;
        internal static void ConfigureAsDefault(TConfiguration configuration) => DefaultConfiguration = configuration;
    }
    internal static class Installer<TConfiguration, TEntity>
        where TConfiguration : IRystemConfiguration
    {
        private static string FullNameConfiguration = typeof(TConfiguration).FullName;
        private static string FullName = typeof(TEntity).FullName;
        private static IDictionary<Installation, TConfiguration> Contexts = new Dictionary<Installation, TConfiguration>();
        internal static void Configure(TConfiguration configuration, Installation installation)
        {
            configuration.Name = configuration.Name ?? typeof(TEntity).FullName;
            if (!Contexts.ContainsKey(installation))
                Contexts.Add(installation, configuration);
            else
                throw new InvalidOperationException($"It already exists an installation for {FullName} in {installation}.");
        }
        internal static TConfiguration GetConfiguration(Installation installation)
        {
            if (Contexts.ContainsKey(installation))
                return Contexts[installation];
            else if (Installer<TConfiguration>.DefaultConfiguration != null)
                return Installer<TConfiguration>.DefaultConfiguration;
            else
                throw new NotImplementedException($"{FullName} never installed. Please use Install static method in static constructor of your class to set ConnectionString and names of table.");
            //throw new NotImplementedException($"Wrong installation type used {installation} instead {string.Join(",", Contexts[installerType.FullName][entity.FullName].Select(x => x.Key))}.");
        }
    }
}
