using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Data
{
    public class AppendBlobBuilder
    {
        public DataConfiguration DataConfiguration { get; }
        public AppendBlobBuilder(string name, IDataReader reader = default, IDataWriter writer = default)
        {
            this.DataConfiguration = new DataConfiguration()
            {
                Name = name,
                Reader = reader,
                Writer = writer,
                Type = DataType.AppendBlob
            };
        }
    }
}