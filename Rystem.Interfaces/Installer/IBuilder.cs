using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    public interface IBuilder
    {
        InstallerType InstallerType { get; }
    }
    public interface IInstallingBuilder : IBuilder
    {
        ConfigurationBuilder Build(Installation installation);
    }
    public interface INoInstallingBuilder : IBuilder
    {
        ConfigurationBuilder Build();
    }
    public interface IBuildingSelector
    {
        ConfigurationBuilder Builder { get; }
    }
}
