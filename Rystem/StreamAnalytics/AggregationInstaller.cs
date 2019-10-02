﻿using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.StreamAnalytics
{
    public static class AggregationInstaller<T>
    {
        private static IDictionary<Installation, AggregationProperty> Installations = new Dictionary<Installation, AggregationProperty>();
        public static void Configure(AggregationProperty aggregationProperty, Installation installation = Installation.Default)
            => Installations.Add(installation, aggregationProperty);
        public static IDictionary<Installation, AggregationProperty> GetProperties()
            => Installations;
    }
}