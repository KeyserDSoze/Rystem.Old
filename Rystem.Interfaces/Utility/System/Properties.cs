﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.Reflection
{
    public static class Properties
    {
        private static Dictionary<string, PropertyInfo[]> AllProperties = new Dictionary<string, PropertyInfo[]>();
        private static object TrafficCard = new object();
        public static PropertyInfo[] Fetch(Type type, params Type[] attributesToIgnore)
        {
            if (!AllProperties.ContainsKey(type.FullName))
                lock (TrafficCard)
                    if (!AllProperties.ContainsKey(type.FullName))
                        AllProperties.Add(type.FullName, type.GetProperties()
                            .Where(x =>
                            {
                                foreach (Type attributeToIgnore in attributesToIgnore)
                                    if (x.GetCustomAttribute(attributeToIgnore) != null)
                                        return false;
                                return true;
                            }).ToArray());
            return AllProperties[type.FullName];
        }
    }
}
