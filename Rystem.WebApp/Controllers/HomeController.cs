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

        [HttpPost]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public IActionResult Index([FromForm]Alo alo) => Ok("A good update");

        public IActionResult Privacy()
        {
            List<CarouselItem> items = new List<CarouselItem>();
            for (int i = 0; i < 10; i++)
                items.Add(new CarouselItem()
                {
                    Content = "https://image.shutterstock.com/z/stock-photo-incredible-nature-landscape-colorful-sky-resia-lake-in-dolomites-mountains-during-sunrise-scenic-1480431083.jpg",
                    Link = "https://www.google.com"
                });
            return View(items);
        }
        public class MyFirstChart : IChart
        {
            public string X => "data";

            public string Y => "numero congiure";

            public string Title => "Bella zi";
            public MyFirstChart()
            {
                this.Datasets = new Dictionary<string, DataModel>();
                for (int i = 0; i < 5; i++)
                {
                    this.Datasets.Add(i.ToString(), new DataModel()
                    {
                        BackgroundColor = "#ff0000" ,
                        BorderColor =  "#ff0000" ,
                        Data = GetNumbers(i),
                        Fill = true,
                        Label = "Circo" + i
                    });
                }
                IEnumerable<decimal> GetNumbers(int start)
                {
                    for (int i = start + 10; i < start + 20; i++)
                        yield return i;
                }
            }
            public Dictionary<string, DataModel> Datasets { get; }
        }

        public IActionResult Charting() 
            => View(new MyFirstChart().ToDataChart());

        public async Task<IActionResult> Rexo()
        {
            await Task.Delay(3000);
            return View();
        }
        static IEnumerable<string> Items;
        public async Task<IActionResult> Cold([FromQuery]IEnumerable<string> selectedItems)
        {
            Items = selectedItems;
            await Task.Delay(3000);
            return View(selectedItems);
        }
        public async Task<IActionResult> More()
        {
            List<Alo> alo = new List<Alo>();
            for (int i = 0; i < 200; i++)
            {
                if (Items == null || !Items.Any(x => x == i.ToString()))
                    alo.Add(new Alo() { A = i.ToString(), Group = (i % 40), Val = i });
            }
            return PartialView("_Ciccia", alo);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
