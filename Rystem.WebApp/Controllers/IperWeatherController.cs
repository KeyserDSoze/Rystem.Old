using Microsoft.AspNetCore.Components;
using Rystem.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rystem.WebApp.Controllers
{
    public class ColdWeather
    {
        public int Id { get; set; }
        public string City { get; set; }
        public int Temperature { get; set; }
    }
    public class IperWeatherController : RepositoryController<ColdWeather>
    {
        private static readonly Dictionary<string, ColdWeather> InMemoryWeather = new Dictionary<string, ColdWeather>()
        {
            { "2", new ColdWeather{ Id = 2, City = "Calgary", Temperature = 25 } }
        };
        protected override Task<EntityResponse> CreateAsync(ColdWeather entity)
        {
            InMemoryWeather.Add(entity.Id.ToString(), entity);
            return Task.FromResult(new EntityResponse { Id = entity.Id.ToString() });
        }

        protected override Task<EntityResponse> DeleteAsync(ColdWeather entity)
        {
            InMemoryWeather.Remove(entity.Id.ToString());
            return Task.FromResult(new EntityResponse { Id = entity.Id.ToString() });
        }

        protected override async Task<ColdWeather> GetAsync(string id)
        {
            await Task.Delay(1000);
            return InMemoryWeather.ContainsKey(id) ? InMemoryWeather[id] : default;
        }

        protected override async Task<IEnumerable<ColdWeather>> ListAsync()
        {
            await Task.Delay(1000);
            return InMemoryWeather.Select(x => x.Value);
        }

        protected override Task<EntityResponse> UpdateAsync(ColdWeather entity)
        {
            if (!InMemoryWeather.ContainsKey(entity.Id.ToString()))
                InMemoryWeather.Add(entity.Id.ToString(), entity);
            else
                InMemoryWeather[entity.Id.ToString()] = entity;
            return Task.FromResult(new EntityResponse { Id = entity.Id.ToString() });
        }
    }
}
