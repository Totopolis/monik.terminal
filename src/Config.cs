using MonikTerminal.Enums;
using MonikTerminal.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace MonikTerminal
{
    public enum TerminalMode
    {
        Stream,
        Single
    }

    public enum MetricTerminalMode
    {
        Current,
        TimeWindow,
    }

    public class Config : IConfig
    {
        public string ServerUrl { get; set; } = "http://url/";

        public string             TimeTemplate             { get; set; } = "HH:mm";
        public string             DoubledTimeTemplate      { get; set; } = "HH:**";
        public int                MaxSourceLen             { get; set; } = 12;
        public int                MaxInstanceLen           { get; set; } = 8;
        public int                MaxMetricName            { get; set; } = 12;
        public int                RefreshPeriod            { get; set; } = 5;
        public int                KeepAliveWarnSeconds     { get; set; } = 60;
        public LevelType          LevelFilter              { get; set; } = LevelType.None;
        public SeverityCutoffType SeverityCutoff           { get; set; } = SeverityCutoffType.None;
        public bool               ShowLevelVerbose         { get; set; } = true;
        public int                Top                      { get; set; } = 25;
        public TerminalMode       Mode                     { get; set; } = TerminalMode.Stream;
        public MetricTerminalMode MetricTerminalMode       { get; set; } = MetricTerminalMode.Current;
        public int                MetricAggWindow5MinWidth { get; set; } = 6; //for half an hour TimeWindow

        public void Load(string aFileName)
        {
            string json = File.ReadAllText(aFileName);

            var                         jobj = JObject.Parse(json);
            IDictionary<string, JToken> dic  = jobj;

            if (dic.ContainsKey("ServerUrl"))
                ServerUrl = (string) dic["ServerUrl"];

            if (dic.ContainsKey("TimeTemplate"))
                TimeTemplate = (string) dic["TimeTemplate"];

            if (dic.ContainsKey("DoubledTimeTemplate"))
                DoubledTimeTemplate = (string) dic["DoubledTimeTemplate"];

            if (dic.ContainsKey("MaxSourceLen"))
                MaxSourceLen = (int) dic["MaxSourceLen"];

            if (dic.ContainsKey("MaxInstanceLen"))
                MaxInstanceLen = (int) dic["MaxInstanceLen"];

            if (dic.ContainsKey("MaxMetricName"))
                MaxMetricName = (int) dic["MaxMetricName"];

            if (dic.ContainsKey("RefreshPeriod"))
                RefreshPeriod = (int) dic["RefreshPeriod"];

            if (dic.ContainsKey("KeepAliveWarnSeconds"))
                KeepAliveWarnSeconds = (int) dic["KeepAliveWarnSeconds"];

            if (dic.ContainsKey("LevelFilter"))
                LevelFilter = Converter.StringToLevelType((string) dic["LevelFilter"]);

            if (dic.ContainsKey("SeverityCutoff"))
                SeverityCutoff = Converter.StringToSeverity((string) dic["SeverityCutoff"]);

            if (dic.ContainsKey("ShowLevelVerbose"))
                ShowLevelVerbose = (bool) dic["ShowLevelVerbose"];

            if (dic.ContainsKey("Top"))
                Top = (int) dic["Top"];

            if (dic.ContainsKey("Mode"))
            {
                string md = (string) dic["Mode"];
                Mode = md == "single" ? TerminalMode.Single : TerminalMode.Stream;
            }

            if (dic.ContainsKey("MetricTerminalMode"))
                MetricTerminalMode = Enum.Parse<MetricTerminalMode>((string) dic["MetricTerminalMode"]);
            
            if (dic.ContainsKey("MetricAggWindow5MinWidth"))
                MetricAggWindow5MinWidth = (int)dic["MetricAggWindow5MinWidth"];

            Console.WriteLine("Config loaded");
        }
    }
}