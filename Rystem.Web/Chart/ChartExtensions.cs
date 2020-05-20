using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rystem.Web
{
    public static class ChartExtensions
    {
        public static DataChart ToDataChart(this IChart entity, ChartType chartType = ChartType.Line)
            => DataChart.Default(entity.Datasets.Keys, entity.Datasets.Select(x => x.Value), entity.Title, entity.X, entity.Y, chartType);
    }
}
