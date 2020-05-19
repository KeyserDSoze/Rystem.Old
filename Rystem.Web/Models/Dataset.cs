using Microsoft.Azure.Amqp.Framing;
using Newtonsoft.Json;
using Rystem.Web;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web
{
    public class DataChart
    {
        public string type => this.Type.ToString().ToLower();
        [JsonIgnore]
        public DatasetType Type { get; set; }
        [JsonProperty("data")]
        public Dataset Data { get; set; }
        [JsonProperty("options")]
        public DataOption Options { get; set; }
        public static DataChart Default(IEnumerable<string> labels, IEnumerable<DataModel> datasets, string title, string xAxes, string yAxes, DatasetType type = DatasetType.Line)
        {
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
    public enum DatasetType
    {
        Line,
        Bar,
    }
    public class DataModel
    {
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("backgroundColor")]
        public string BackgroundColor { get; set; }
        [JsonProperty("borderColor")]
        public string BorderColor { get; set; }
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
