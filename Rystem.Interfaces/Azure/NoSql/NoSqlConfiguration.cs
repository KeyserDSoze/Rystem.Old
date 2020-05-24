using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.NoSql
{
    public class NoSqlConfiguration : IConfiguration
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public NoSqlType Type { get; set; }
    }
}
