﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Rystem.Utility
{
    public static class Alea
    {
        public static string GetTimedKey()
        {
            return string.Format("{0:d19}{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));
        }
        public static int GetNumber(int maxNotIncluding)
        {
            byte[] randomNumber = new byte[1];
            RNGCryptoServiceProvider gen = new RNGCryptoServiceProvider();
            gen.GetBytes(randomNumber);
            int rand = Convert.ToInt32(randomNumber[0]) * maxNotIncluding / 255;
            if (rand == maxNotIncluding)
                return rand - 1;
            return rand;
        }
    }
}
