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
    public class RepositoryPatternOption
    {
        private readonly List<IRepositoryCache> Caches = new List<IRepositoryCache>();
        private RepositoryCache<object> DefaultCache;
        private bool HasDefaultCache => DefaultCache != null;
        internal IRepositoryCache GetCache<TModel>()
        {
            Type type = typeof(TModel);
            IRepositoryCache repositoryCache = Caches.FirstOrDefault(x => x.ModelType == type);
            if (repositoryCache != null)
                return repositoryCache;
            else if (HasDefaultCache)
                return DefaultCache;
            else
                return default;
        }
        public CacheSelector AddDefaultCache(CacheConsistency cacheConsistency)
        {
            DefaultCache = new RepositoryCache<object>()
            {
                CacheConfiguration = new ConfigurationBuilder()
            };
            return DefaultCache.CacheConfiguration.WithCache(cacheConsistency);
        }
        public CacheSelector AddCache<TModel>(CacheConsistency cacheConsistency)
            where TModel : new()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var repositoryCache = new RepositoryCache<TModel>()
            {
                CacheConfiguration = configurationBuilder
            };
            var repositoryCacheAsList = new RepositoryCache<IEnumerable<TModel>>()
            {
                CacheConfiguration = configurationBuilder
            };
            Caches.Add(repositoryCache);
            Caches.Add(repositoryCacheAsList);
            return repositoryCache.CacheConfiguration.WithCache(cacheConsistency);
        }
    }
}