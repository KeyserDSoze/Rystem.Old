using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.RepositoryPattern
{
    public class RepositoryRequestTest : IUnitTest
    {
        public class Weather
        {
            public int Id { get; set; }
            public string City { get; set; }
            public int Temperature { get; set; }
        }
        public class WeatherApi : IRepositoryCaller
        {
            public Uri Uri => new Uri("https://testy.azurewebsites.net/api/weather");
            public RepositoryPatternErrorResponse ErrorResponse { get; set; }
        }
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            WeatherApi Api = new WeatherApi();
            var weather = await Api.GetAsync<Weather>("2");
            await Api.CreateAsync(new Weather
            {
                Id = 6,
                City = "Falado",
                Temperature = 45
            });
            var weatherList = await Api.ListAsync<Weather>();
            await Api.UpdateAsync(new Weather
            {
                Id = 6,
                City = "Falado",
                Temperature = 45
            });
            weatherList = await Api.ListAsync<Weather>();
            await Api.DeleteAsync("6");
            WeatherApi Api2 = new WeatherApi();
            await Api2.DeleteAsync("6");
            weatherList = await Api.ListAsync<Weather>();
        }
    }
}
