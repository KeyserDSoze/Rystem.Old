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
    public class RepositoryCacheKey<TModel> : ICacheKey<TModel>
    {
        public string Key { get; set; }
        public async Task<TModel> FetchAsync()
            => await this.Fetcher.Invoke().NoContext();
        public Func<Task<TModel>> Fetcher;
        internal ConfigurationBuilder ConfigurationBuilder { get; set; }
        public ConfigurationBuilder GetConfigurationBuilder()
            => ConfigurationBuilder;
    }
}