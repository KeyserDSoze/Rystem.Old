﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.NoSql
{
    public class TableStorageBuilder
    {
        public NoSqlConfiguration NoSqlConfiguration { get; }
        public TableStorageBuilder(string name)
        {
            this.NoSqlConfiguration = new NoSqlConfiguration()
            {
                Name = name,
                Type = NoSqlType.TableStorage
            };
        }
    }
}
