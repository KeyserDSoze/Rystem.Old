using Newtonsoft.Json;
using Rystem.Azure.AggregatedData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.ZTester
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }

    public class Newspaper
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }

    public class CrostaModel :  IAggregatedData
    {
        public int Id { get; set; }
        public string ExternalId { get; set; }
        public string Article { get; set; }
        public string Title { get; set; }
        public string SecondTitle { get; set; }
        public string Author { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public DateTime Published { get; set; }
        public int NewsType { get; set; }
        public string Main_Image { get; set; }
        public Category Category { get; set; }
        public Newspaper Newspaper { get; set; }
        public string Subtitle { get => SecondTitle; set { } }
        public string NewsId { get => ExternalId; set { } }
        public DateTime PubDate { get => Updated; set { } }
        public string Description { get => Article; set { } }
        public string ImageName { get => Main_Image; set { } }
        public string ExternalUrl { get => ""; set { } }

        public string Name { get; set; }
        public AggregatedDataProperties Properties { get; set; }

        static CrostaModel()
        {
            AggregatedDataInstaller.Configure<CrostaModel>(new AggregatedDataConfiguration<CrostaModel>()
            {
                ConnectionString = "DefaultEndpointsProtocol=https;AccountName=h3gbroadband;AccountKey=GNA22K3Wi7vv2ebp+OtSIHyR0UIk81GKGdR3nF5Ydkgj6Q5TZ69oa4r0UC7wqXC9p/iZa0c/tc7IPyUP9R0GjA==;EndpointSuffix=core.windows.net",
                Name = "newsdetailsfeed",
                Reader = new CrostaReader(),
                Type = AggregatedDataType.BlockBlob
            });
        }
    }
    public class CrostaReader : IAggregatedDataReader<CrostaModel>
    {
        public async Task<CrostaModel> ReadAsync(AggregatedDataDummy dummy)
        {
            using (StreamReader stream = new StreamReader(dummy.Stream))
            {
                string news = await stream.ReadToEndAsync();
                CrostaModel crostaModel = JsonConvert.DeserializeObject<CrostaModel>(news);
                return crostaModel;
            }
        }
    }

}
