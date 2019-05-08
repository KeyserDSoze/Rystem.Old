using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class NoMultitonKey : Attribute { }
    public interface IMultitonKey
    {
    }
}
