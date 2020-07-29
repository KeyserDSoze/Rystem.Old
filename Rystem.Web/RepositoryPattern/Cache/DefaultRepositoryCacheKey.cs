using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Web
{
    public class DefaultRepositoryCacheKey : ICacheKey<object>
    {
        public Task<object> FetchAsync()
            => default;
        internal ConfigurationBuilder ConfigurationBuilder { get; set; }
        public ConfigurationBuilder GetConfigurationBuilder()
            => ConfigurationBuilder;
    }
}