using Newtonsoft.Json;
using Rystem.Web;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Rystem.Web
{
    public class DataChart
    {
        public string type => this.Type.ToString().ToLower();
        [JsonIgnore]
        public ChartType Type { get; set; }
        [JsonProperty("data")]
        public Dataset Data { get; set; }
        [JsonProperty("options")]
        public DataOption Options { get; set; }
        private const int NumberOfColor = 10;
        private static readonly Color[] Colors = new Color[10]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Cyan,
            Color.Magenta,
            Color.Gold,
            Color.DeepPink,
            Color.Beige,
            Color.Azure,
            Color.DarkViolet,
        };
        public static DataChart Default(IEnumerable<string> labels, IEnumerable<DataModel> datasets, string title, string xAxes, string yAxes, ChartType type = ChartType.Line)
        {
            int count = 0;
            foreach (DataModel dataModel in datasets)
            {
                if (dataModel.BackgroundColor == default)
                    dataModel.BackgroundColor = Colors[count % NumberOfColor];
                if (dataModel.BorderColor == default)
                    dataModel.BorderColor = Colors[count % NumberOfColor];
                count++;
            }
            return new DataChart()
            {
                Type = type,
                Data = new Dataset()
                {
                    Labels = labels,
                    Datasets = datasets,
                },
                Options = new DataOption()
                {
                    Hover = new DataHover(),
                    Title = new DataTitle()
                    {
                        Text = title
                    },
                    Tooltips = new DataTooltip(),
                    Scales = new DataScale()
                    {
                        XAxes = new DataAxes[1] {
                            new DataAxes()
                            {
                                Display = true,
                                ScaleLabel = new DataLabel()
                                {
                                    Display = true,
                                    Label = xAxes
                                }
                            }
                        },
                        YAxes = new DataAxes[1] {
                            new DataAxes()
                            {
                                Display = true,
                                ScaleLabel = new DataLabel()
                                {
                                    Display = true,
                                    Label = yAxes
                                }
                            }
                        }
                    }
                }
            };
        }
    }
    public class Dataset
    {
        [JsonProperty("labels")]
        public IEnumerable<string> Labels { get; set; }
        [JsonProperty("datasets")]
        public IEnumerable<DataModel> Datasets { get; set; }
    }
    public enum ChartType
    {
        Line,
        Bar,
        Pie
    }
    public class DataModel
    {
        [JsonProperty("label")]
        public string Label { get; set; }
        public string backgroundColor => $"#{this.BackgroundColor.R:X2}{this.BackgroundColor.G:X2}{this.BackgroundColor.B:X2}";
        [JsonIgnore]
        public Color BackgroundColor { get; set; }
        public string borderColor => $"#{this.BorderColor.R:X2}{this.BorderColor.G:X2}{this.BorderColor.B:X2}";
        [JsonIgnore]
        public Color BorderColor { get; set; }
        [JsonProperty("data")]
        public IEnumerable<decimal> Data { get; set; }
        [JsonProperty("fill")]
        public bool Fill { get; set; }
    }
    public class DataTitle
    {
        [JsonProperty("display")]
        public bool Display { get; set; } = true;
        [JsonProperty("text")]
        public string Text { get; set; }
    }
    public class DataTooltip
    {
        [JsonProperty("mode")]
        public string Mode { get; set; } = "index";
        [JsonProperty("intersect")]
        public bool Intersect { get; set; }
    }
    public class DataHover
    {
        [JsonProperty("mode")]
        public string Mode { get; set; } = "nearest";
        [JsonProperty("intersect")]
        public bool Intersect { get; set; }
    }
    public class DataAxes
    {
        [JsonProperty("display")]
        public bool Display { get; set; }
        [JsonProperty("scaleLabel")]
        public DataLabel ScaleLabel { get; set; }
    }
    public class DataLabel
    {
        [JsonProperty("display")]
        public bool Display { get; set; }
        [JsonProperty("labelString")]
        public string Label { get; set; }
    }
    public class DataScale
    {
        [JsonProperty("xAxes")]
        public IEnumerable<DataAxes> XAxes { get; set; }
        [JsonProperty("yAxes")]
        public IEnumerable<DataAxes> YAxes { get; set; }
    }
    public class DataOption
    {
        [JsonProperty("responsive")]
        public bool Responsive { get; set; } = true;
        [JsonProperty("maintainAspectRatio")]
        public bool MaintainAspectRatio { get; set; }
        [JsonProperty("title")]
        public DataTitle Title { get; set; }
        [JsonProperty("tooltips")]
        public DataTooltip Tooltips { get; set; }
        [JsonProperty("hover")]
        public DataHover Hover { get; set; }
        [JsonProperty("scales")]
        public DataScale Scales { get; set; }
    }
}
