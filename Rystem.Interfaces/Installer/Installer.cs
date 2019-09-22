using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem
{
    internal static class Installer<TProperty>
    {
        internal static TProperty DefaultContexts;
        internal static void ConfigureAsDefault(TProperty property) => DefaultContexts = property;
    }
    internal static class Installer<TProperty, TEntity>
    {
        private static string FullNameProperty = typeof(TProperty).FullName;
        private static string FullName = typeof(TEntity).FullName;
        private static IDictionary<Installation, TProperty> Contexts = new Dictionary<Installation, TProperty>();
        internal static void Configure(TProperty property, Installation installation)
        {
            if (!Contexts.ContainsKey(installation))
                Contexts.Add(installation, property);
            else
                throw new InvalidOperationException($"It already exists an installation for {FullName} in {installation}.");
        }
        internal static TProperty GetConfiguration(Installation installation)
        {
            if (Contexts.ContainsKey(installation))
                return Contexts[installation];
            else if (Installer<TProperty>.DefaultContexts != null)
                return Installer<TProperty>.DefaultContexts;
            else
                throw new NotImplementedException($"{FullName} never installed. Please use Install static method in static constructor of your class to set ConnectionString and names of table.");
            //throw new NotImplementedException($"Wrong installation type used {installation} instead {string.Join(",", Contexts[installerType.FullName][entity.FullName].Select(x => x.Key))}.");
        }
    }
}
