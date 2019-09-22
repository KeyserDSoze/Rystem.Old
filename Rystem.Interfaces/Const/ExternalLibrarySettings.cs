using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Interfaces.Const
{
    public static class ExternalLibrarySettings
    {
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
    }
}
