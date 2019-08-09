using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Rystem.Interfaces.Utility.Crypting
{
    public static class SHA256Helper
    {
        public static string Sum(string value)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                byte[] bytes = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(value));
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var @byte in bytes)
                    stringBuilder.Append(@byte.ToString("x2"));
                return stringBuilder.ToString();
            }
        }
    }
}
