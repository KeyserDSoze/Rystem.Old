﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.DataLake
{
    public interface IDataLakeWriter
    {
        DataLakeDummy Write(IDataLake entity);
    }
}
