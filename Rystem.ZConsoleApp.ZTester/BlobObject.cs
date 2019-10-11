using Microsoft.WindowsAzure.Storage;
using Rystem.Azure.AggregatedData;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Reporting.WindTre.Library.Base.Blob
{
    public class BlobObject : IAggregatedData
    {
        static BlobObject()
        {
            AggregatedDataInstaller.Configure<BlobObject>(new AggregatedDataConfiguration<BlobObject>()
            {
                ConnectionString = "DefaultEndpointsProtocol=https;AccountName=storagetoolreporting;AccountKey=/IBCbY/7K93GVsmX42hdbGbve83QbwWwEkjM0+W6KvG0PLjFMDCQAcTDNDpCiUJbqELG+a68T8k49KFrtwjuiA==;EndpointSuffix=core.windows.net",
                ListReader = null,
                Name = "outputh3g",
                Reader = new BlobManager<BlobObject>(),
                Type = AggregatedDataType.BlockBlob,
                Writer = new BlobManager<BlobObject>(),
            }, Installation.Inst00);
            AggregatedDataInstaller.Configure<BlobObject>(new AggregatedDataConfiguration<BlobObject>()
            {
                ConnectionString = "DefaultEndpointsProtocol=https;AccountName=storagetoolreporting;AccountKey=/IBCbY/7K93GVsmX42hdbGbve83QbwWwEkjM0+W6KvG0PLjFMDCQAcTDNDpCiUJbqELG+a68T8k49KFrtwjuiA==;EndpointSuffix=core.windows.net",
                ListReader = null,
                Name = "outputwind",
                Reader = new BlobManager<BlobObject>(),
                Type = AggregatedDataType.BlockBlob,
                Writer = new BlobManager<BlobObject>(),
            }, Installation.Inst01);
        }

        public Stream Content { get; set; }
        public string Name { get; set; } = "Output";
        public AggregatedDataProperties Properties { get; set; }
    }
}

