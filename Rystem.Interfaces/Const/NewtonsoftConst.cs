using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Const
{
    public static class NewtonsoftConst
    {
        public static readonly JsonSerializerSettings AutoNameHandling_NullIgnore_JsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore
        };
    }
}
