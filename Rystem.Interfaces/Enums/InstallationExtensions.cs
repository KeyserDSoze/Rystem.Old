﻿using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class InstallationExtensions
    {
        public static Installation ToInstallation(this Enum t)
            => (Installation)t;
    }
}
