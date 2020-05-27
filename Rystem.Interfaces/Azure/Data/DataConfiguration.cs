using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Data
{
    public class DataConfiguration : IConfiguration
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public DataType Type { get; set; }
        public IDataReader Reader { get; set; }
        public IDataWriter Writer { get; set; }
        internal DataConfiguration() { }
    }
}
