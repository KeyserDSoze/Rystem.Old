using Rystem.NoSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.NoSql
{
    public abstract class BlobStorage : INoSql
    {
        public List<string> Keys { get; set; }
        public abstract ConfigurationBuilder GetConfigurationBuilder();
    }
}