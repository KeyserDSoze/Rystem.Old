using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Data
{
    public class DataLakeV2Builder<TData>
        where TData : IData
    {
        public DataConfiguration<TData> DataConfiguration { get; }
        public DataLakeV2Builder(string name, IDataReader<TData> reader = default, IDataWriter<TData> writer = default)
        {
            this.DataConfiguration = new DataConfiguration<TData>()
            {
                Name = name,
                Reader = reader,
                Writer = writer,
                Type = DataType.DataLakeV2
            };
        }
    }
}
