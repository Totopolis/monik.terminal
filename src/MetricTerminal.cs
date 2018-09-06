using MonikTerminal.Interfaces;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonikTerminal
{
    public class MetricTerminal : IMetricTerminal
    {
        private readonly IMonikService _service;
        private readonly IConfig       _config;
        private readonly ISourcesCache _sourceCache;

        public MetricTerminal(IMonikService aService, IConfig aConfig, ISourcesCache aSourceCache)
        {
            _service     = aService;
            _config      = aConfig;
            _sourceCache = aSourceCache;
        }

        public void Start(string configFileName)
        {
            var json = File.ReadAllText(configFileName);
            var metricConfig = JsonConvert.DeserializeObject<MetricConfig>(json);

            Console.Clear();
            Console.Title =
                $"{nameof(MonikTerminal)}: {nameof(MetricTerminal)}";

            while (true)
            {
                try
                {
                    var windows = _service.GetMetricsWindow().Result;
                    var metrics =
                        from metric in _sourceCache.Metrics
                        join config in metricConfig.Metrics on metric.ID equals config.MetricId
                        join window in windows on metric.ID equals window.MetricId
                        select new {metric, window, config};

                    Console.SetCursorPosition(0, 0);

                    foreach (var data in metrics)
                    {
                        var metric = data.metric;
                        var instance = metric.Instance;

                        var instName = Converter.Truncate(instance.Name, _config.MaxInstanceLen);
                        var srcName = Converter.Truncate(instance.Source.Name, _config.MaxSourceLen);
                        var metName = Converter.Truncate(metric.Name, metricConfig.MaxMetricLen);

                        var str = string.Format($"{{0,-{_config.MaxSourceLen}}} {{1,-{_config.MaxInstanceLen}}} {{2, -{metricConfig.MaxMetricLen}}} | ",
                            srcName,
                            instName,
                            metName);

                        Console.Write(str);

                        var value = data.window.Value;
                        foreach (var boundary in data.config.Areas)
                        {
                            if (boundary.Range.Length != 2)
                                throw new ArgumentException("Wrong Range format");

                            var a = boundary.Range[0];
                            var b = boundary.Range[1];

                            if ((!a.HasValue || value >= a.Value) &&
                                (!b.HasValue || value < b.Value))
                            {
                                Console.BackgroundColor =
                                    !string.IsNullOrEmpty(boundary.Color)
                                        ? Enum.Parse<ConsoleColor>(boundary.Color)
                                        : ConsoleColor.Black;

                                var valString = value.ToString(data.config.ValueFormat ?? metricConfig.DefaultValueFormat);
                                valString = valString.Length <= metricConfig.MaxMetricValueLen ? valString : valString.Substring(valString.Length - metricConfig.MaxMetricValueLen);
                                Console.Write($"{{0,{metricConfig.MaxMetricValueLen}}}", valString);

                                Console.BackgroundColor = ConsoleColor.Black;
                                break;
                            }
                        }

                        Console.WriteLine();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"INTERNAL ERROR: {ex.Message}");
                }

                if (_config.Mode == TerminalMode.Single)
                    return;

                Task.Delay(_config.RefreshPeriod * 1000).Wait();
            } //while true
        }
    }
}