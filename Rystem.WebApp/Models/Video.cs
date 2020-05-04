using Rystem.Azure.AggregatedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rystem.WebApp.Models
{
    public class Video : IAggregatedData
    {
        static Video()
            => AggregatedDataInstaller.Configure<Video>(new AggregatedDataConfiguration<Video>()
            {
                ConnectionString = "DefaultEndpointsProtocol=https;AccountName=rystem;AccountKey=OeOn4ae4HmlWjJCE1wGZRCsrPsjGwtMr0tjte3F+TAQ4sSA+uNiBQtKrgwI+RxlkF60IBOwI9J7qe3wPFSvm8A==;EndpointSuffix=core.windows.net",
                Type = AggregatedDataType.BlockBlob
            });
        public string Name { get; set; }
        public AggregatedDataProperties Properties { get; set; }
    }
}
