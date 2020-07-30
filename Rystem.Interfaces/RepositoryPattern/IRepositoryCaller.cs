using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Rystem;

namespace System
{
    public interface IRepositoryCaller
    {
        Uri Uri { get; }
        RepositoryPatternErrorResponse ErrorResponse { get; set; }
        public bool HasError => ErrorResponse != null;
    }
}