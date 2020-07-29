using Microsoft.AspNetCore.Components;
using Rystem.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rystem.WebApp.Controllers
{
    public class Weather
    {
        public int Id { get; set; }
        public string City { get; set; }
        public int Temperature { get; set; }
    }
    public class WeatherController : RepositoryController<Weather>
    {
        private static readonly Dictionary<string, Weather> InMemoryWeather = new Dictionary<string, Weather>()
        {
            { "2", new Weather{ Id = 2, City = "Rome", Temperature = 23 } }
        };
        protected override Task<EntityResponse> CreateAsync(Weather entity)
        {
            InMemoryWeather.Add(entity.Id.ToString(), entity);
            return Task.FromResult(new EntityResponse { Id = entity.Id.ToString() });
        }

        protected override Task<EntityResponse> DeleteAsync(string id)
        {
            bool exists = InMemoryWeather.ContainsKey(id);
            if (exists)
                InMemoryWeather.Remove(id);
            return Task.FromResult(new EntityResponse { Id = id, IsNotOk = !exists });
        }

        protected override async Task<Weather> GetAsync(string id)
        {
            await Task.Delay(2000);
            return InMemoryWeather.ContainsKey(id) ? InMemoryWeather[id] : default;
        }

        protected override async Task<IEnumerable<Weather>> ListAsync()
        {
            await Task.Delay(2000);
            return InMemoryWeather.Select(x => x.Value);
        }

        protected override Task<EntityResponse> UpdateAsync(Weather entity)
        {
            if (!InMemoryWeather.ContainsKey(entity.Id.ToString()))
                InMemoryWeather.Add(entity.Id.ToString(), entity);
            else
                InMemoryWeather[entity.Id.ToString()] = entity;
            return Task.FromResult(new EntityResponse { Id = entity.Id.ToString() });
        }
    }
}
