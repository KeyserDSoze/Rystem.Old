using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    public interface IConfigurator
    {
        ConfigurationBuilder GetConfigurationBuilder();
    }
}
