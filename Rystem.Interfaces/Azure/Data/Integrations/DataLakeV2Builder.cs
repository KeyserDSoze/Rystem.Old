using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Data
{
    public class DataLakeV2Builder
    {
        public DataConfiguration DataConfiguration { get; }
        public DataLakeV2Builder(string name, IDataReader reader = default, IDataWriter writer = default)
        {
            this.DataConfiguration = new DataConfiguration()
            {
                Name = name,
                Reader = reader,
                Writer = writer,
                Type = DataType.DataLakeV2
            };
        }
    }
}
