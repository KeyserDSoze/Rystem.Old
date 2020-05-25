using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Data
{
    public class DataConfiguration<Entity> : IConfiguration
        where Entity : IData
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public DataType Type { get; set; }
        public IDataReader<Entity> Reader { get; set; }
        public IDataWriter<Entity> Writer { get; set; }
        internal DataConfiguration() { }
    }
}
