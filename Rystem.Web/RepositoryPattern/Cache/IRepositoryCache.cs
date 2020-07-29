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
    public interface IRepositoryCache
    {
        Type ModelType { get; }
        ConfigurationBuilder CacheConfiguration { get; set; }
        bool IsDefault { get; }
    }
}