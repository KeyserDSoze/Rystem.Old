using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Web.Models
{
    public class FileModel
    {
        public string FullName { get; set; }
        public string ContentType { get; set; }
        public IEnumerable<string> GetPath()
        {
            string[] value = this.FullName.Split('/');
            return value.Skip(3);
        }
    }
}