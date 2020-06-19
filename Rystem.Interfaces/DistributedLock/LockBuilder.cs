using System;

namespace Rystem.DistributedLock
{
    public class LockBuilder : IInstallingBuilder
    {
        private readonly IConfiguration LockConfiguration;
        private readonly LockSelector LockSelector;
        internal LockBuilder(IConfiguration lockConfiguration, LockSelector lockSelector)
        {
            this.LockConfiguration = lockConfiguration;
            this.LockSelector = lockSelector;
        }
        public InstallerType InstallerType => InstallerType.Lock;

        public ConfigurationBuilder Build(Installation installation = Installation.Default)
        {
            this.LockSelector.Builder.AddConfiguration(this.LockConfiguration, this.InstallerType, installation);
            return this.LockSelector.Builder;
        }
    }
}