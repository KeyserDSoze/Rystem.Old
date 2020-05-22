using Rystem.Azure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rystem.WebApp.Models
{
    public class Video : IData
    {
        static Video()
            => DataInstaller.Configure<Video>(new DataConfiguration<Video>()
            {
                ConnectionString = "DefaultEndpointsProtocol=https;AccountName=rystem;AccountKey=OeOn4ae4HmlWjJCE1wGZRCsrPsjGwtMr0tjte3F+TAQ4sSA+uNiBQtKrgwI+RxlkF60IBOwI9J7qe3wPFSvm8A==;EndpointSuffix=core.windows.net",
                Type = AggregatedDataType.BlockBlob
            });
        public string Name { get; set; }
        public IDataProperties Properties { get; set; }
    }
    public class Alo
    {
        public string A { get; set; }
        public int Val { get; set; }
        public int Group { get; set; }
    }
}
