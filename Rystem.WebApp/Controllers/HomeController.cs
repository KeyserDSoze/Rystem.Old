using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rystem.Web;
using Rystem.WebApp.Models;

namespace Rystem.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            List<Alo> alo = new List<Alo>();
            for (int i = 0; i < 200; i++)
            {
                alo.Add(new Alo() { A = i.ToString(), Group = (i % 40), Val = i });
            }
            return View(alo);
        }

        public IActionResult Privacy()
        {
            return View();
        }


        public async Task<IActionResult> Rexo()
        {
            await Task.Delay(3000);
            return View();
        }
        public async Task<IActionResult> Cold([FromQuery]IEnumerable<string> selectedItems)
        {
            await Task.Delay(3000);
            return View(selectedItems);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
