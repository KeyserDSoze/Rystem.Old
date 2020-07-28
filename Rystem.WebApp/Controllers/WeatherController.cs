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
        protected override Task CreateAsync(Weather entity)
        {
            InMemoryWeather.Add(entity.Id.ToString(), entity);
            return Task.CompletedTask;
        }

        protected override Task DeleteAsync(Weather entity)
        {
            InMemoryWeather.Remove(entity.Id.ToString());
            return Task.CompletedTask;
        }

        protected override Task<Weather> GetAsync(string id)
            => Task.FromResult(InMemoryWeather[id]);

        protected override Task<IEnumerable<Weather>> ListAsync()
            => Task.FromResult(InMemoryWeather.Select(x => x.Value));

        protected override Task UpdateAsync(Weather entity)
        {
            if (!InMemoryWeather.ContainsKey(entity.Id.ToString()))
                InMemoryWeather.Add(entity.Id.ToString(), entity);
            else
                InMemoryWeather[entity.Id.ToString()] = entity;
            return Task.CompletedTask;
        }
    }
}
