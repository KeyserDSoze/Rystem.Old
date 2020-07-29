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
    public class RepositoryCache<TModel> : IRepositoryCache
    {
        public ConfigurationBuilder CacheConfiguration { get; set; }
        public Type ModelType => typeof(TModel);
        public bool IsDefault => ModelType == typeof(object);
    }
}