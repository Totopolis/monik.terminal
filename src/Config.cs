using System;
using MonikTerminal.Enums;
using MonikTerminal.Interfaces;
using Newtonsoft.Json;

namespace MonikTerminal
{
    public enum TerminalMode
    {
        Stream,
        Single
    }

    public class MetricConfigArea
    {
        public double?[] Range { get; set; }

        [JsonIgnore] public ConsoleColor? Color { get; set; }
        [JsonProperty("Color")]
        public string TmpColor
        {
            get => Color?.ToString();
            set => Color = Converter.StringToConsoleColor(value);
        }

        [JsonIgnore] public ConsoleColor? FontColor { get; set; }
        [JsonProperty("FontColor")]
        public string TmpFontColor
        {
            get => FontColor?.ToString();
            set => FontColor = Converter.StringToConsoleColor(value);
        }
    }

    public class MetricConfigValue
    {
        public long MetricId { get; set; }
        public string Name { get; set; }
        public string ValueFormat { get; set; }
        public MetricConfigArea[] Areas { get; set; } = {};
    }

    public class MetricsConfig
    {
        public int MaxMetricLen { get; set; } = 16;
        public int MaxMetricValueLen { get; set; } = 8;
        public int MaxAggregationTypeLen { get; set; } = 3;
        public string DefaultValueFormat { get; set; } = "0.#";
        public MetricConfigValue[] Metrics { get; set; } = {};
        public MetricConfigValue[] MetricsAutoFilled { get; set; } = {};
    }

    public class CommonConfig
    {
        public string TimeTemplate { get; set; } = "HH:mm";

        public string ServerUrl { get; set; } = "http://url/";
        public int RefreshPeriod { get; set; } = 5;

        [JsonIgnore] public TerminalMode Mode { get; set; } = TerminalMode.Stream;

        [JsonProperty("Mode")]
        public string TmpMode
        {
            get => Converter.TerminalModeToString(Mode);
            set => Mode = Converter.StringToTerminalMode(value);
        }
    }

    public class LogConfig
    {
        public string DoubledTimeTemplate { get; set; } = "HH:**";

        public int MaxSourceLen { get; set; } = 12;
        public int MaxInstanceLen { get; set; } = 8;

        public bool ShowLevelVerbose { get; set; } = true;
        public int Top { get; set; } = 25;

        [JsonIgnore] public LevelType LevelFilter { get; set; } = LevelType.None;

        [JsonProperty("LevelFilter")]
        public string TmpLevelFilter
        {
            get => Converter.LevelTypeToString(LevelFilter);
            set => LevelFilter = Converter.StringToLevelType(value);
        }

        [JsonIgnore] public SeverityCutoffType SeverityCutoff { get; set; } = SeverityCutoffType.None;

        [JsonProperty("SeverityCutoff")]
        public string TmpSeverityCutoff
        {
            get => Converter.SeverityToString(SeverityCutoff);
            set => SeverityCutoff = Converter.StringToSeverity(value);
        }
    }

    public class KeepAliveConfig
    {
        public int MaxSourceLen { get; set; } = 12;
        public int MaxInstanceLen { get; set; } = 8;

        public int KeepAliveWarnSeconds { get; set; } = 60;
    }

    public class MetricsFillConfig
    {
        public string ValueFormatAccum { get; set; } = null;
        public string ValueFormatGauge { get; set; } = "0.#";
        public string Areas { get; set; } =
            @"[
      {""Range"": [null, 0], ""Color"": ""DarkGreen"", ""FontColor"": ""Black""},
      {""Range"": [0, 1], ""Color"": ""DarkYellow""},
      {""Range"": [1, null], ""Color"": ""DarkRed""}
    ]";
    }

    public class Config : IConfig
    {
        public static Config Default()
        {
            return new Config
            {
                Common = new CommonConfig(),
                Log = new LogConfig(),
                KeepAlive = new KeepAliveConfig(),
                Metrics = new MetricsConfig()
            };
        }

        public CommonConfig Common { get; set; }
        public LogConfig Log { get; set; }
        public KeepAliveConfig KeepAlive { get; set; }
        public MetricsConfig Metrics { get; set; }
        public MetricsFillConfig MetricsFill { get; set; }
    }
}