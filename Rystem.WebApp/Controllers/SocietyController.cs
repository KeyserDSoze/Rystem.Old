using Microsoft.AspNetCore.Components;
using Rystem.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rystem.WebApp.Controllers
{
    public class Society
    {
        public int Id { get; set; }
        public string City { get; set; }
        public int Value { get; set; }
    }
    public class SocietyController : RepositoryController<Society>
    {
        private static readonly Dictionary<string, Society> InMemoryWeather = new Dictionary<string, Society>()
        {
            { "2", new Society{ Id = 2, City = "Fdalda", Value = 25 } }
        };
        protected override Task<EntityResponse> CreateAsync(Society entity)
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

        protected override async Task<Society> GetAsync(string id)
        {
            await Task.Delay(1000);
            return InMemoryWeather.ContainsKey(id) ? InMemoryWeather[id] : default;
        }

        protected override async Task<IEnumerable<Society>> ListAsync()
        {
            await Task.Delay(1000);
            return InMemoryWeather.Select(x => x.Value);
        }

        protected override Task<EntityResponse> UpdateAsync(Society entity)
        {
            if (!InMemoryWeather.ContainsKey(entity.Id.ToString()))
                InMemoryWeather.Add(entity.Id.ToString(), entity);
            else
                InMemoryWeather[entity.Id.ToString()] = entity;
            return Task.FromResult(new EntityResponse { Id = entity.Id.ToString() });
        }
    }
}
