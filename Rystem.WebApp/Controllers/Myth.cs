using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Rystem.Web.Backoffice;
using Rystem.WebApp.Models;

namespace Rystem.WebApp.Controllers
{
    public class Soluto
    {
        public FolieriType FolieriType { get; set; }
        public static IEnumerable<Soluto> Get()
        {
            return new List<Soluto>()
            {
                new Soluto(),
                new Soluto(),
                new Soluto(),
                new Soluto(),
                new Soluto(),
                new Soluto(),
                new Soluto(),
                new Soluto(),
                new Soluto(),
            };
        }
    }
    public enum FolieriType
    {
        Moldo,
        Coldo,
        Boldo
    }
    public class MythController : BackOfficeController<Soluto>
    {
        private readonly IStringLocalizer<DatabaseShared> StringLocalizer;
        public MythController(IStringLocalizer<DatabaseShared> stringLocalizer)
        {
            this.StringLocalizer = stringLocalizer;
        }
        public override INavigation<Soluto> GetDeleteNavigation()
        {
            throw new NotImplementedException();
        }

        public override INavigation<Soluto> GetIndexNavigation()
        {
            return INavigation<Soluto>
                .Create(NavigationOptions.CanDoneAll("D", this.StringLocalizer))
                .Include(x => x.FolieriType, new PropertyOptions() { IsLocalized = true });
        }

        public override Task<IEnumerable<Soluto>> GetList()
            => Task.FromResult(Soluto.Get());

        public override Task<Soluto> GetModel(string id)
            => Task.FromResult(new Soluto());

        public override Task<bool> Remove(string id)
        {
            throw new NotImplementedException();
        }
    }
}
