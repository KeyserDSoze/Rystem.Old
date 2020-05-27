using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class CacheBuilder : INoInstallingBuilder
    {
        private readonly IConfiguration Configuration;
        private readonly CacheSelector CacheSelector;
        internal CacheBuilder(IConfiguration configuration, CacheSelector cacheSelector)
        {
            this.Configuration = configuration;
            this.CacheSelector = cacheSelector;
        }
        public InstallerType InstallerType => InstallerType.Cache;
        public ConfigurationBuilder Build()
        {
            this.CacheSelector.Builder.AddConfiguration(this.Configuration, this.InstallerType, Installation.Default);
            return this.CacheSelector.Builder;
        }
        public CacheSelector And()
            => this.CacheSelector;
    }
}
