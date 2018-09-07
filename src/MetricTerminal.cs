using MonikTerminal.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonikTerminal
{
    public class MetricTerminal : IMetricTerminal
    {
        private readonly IMonikService _service;
        private readonly IConfig       _config;
        private readonly ISourcesCache _sourceCache;

        private int _windowWidth;

        public MetricTerminal(IMonikService aService, IConfig aConfig, ISourcesCache aSourceCache)
        {
            _service     = aService;
            _config      = aConfig;
            _sourceCache = aSourceCache;
        }

        public void Start()
        {
            var configMetrics = _config.Metrics;

            Console.Clear();
            Console.Title =
                $"{nameof(MonikTerminal)}: {nameof(MetricTerminal)}";

            while (true)
            {
                try
                {
                    var windows = _service.GetMetricsWindow().Result;
                    var metrics =
                        from config in configMetrics.Metrics
                        join metric in _sourceCache.Metrics on config.MetricId equals metric.ID
                        join window in windows on metric.ID equals window.MetricId
                        select new {metric, window, config};

                    if (_windowWidth != Console.WindowWidth)
                    {
                        _windowWidth = Console.WindowWidth;
                        Console.Clear();
                    }
                    else
                        Console.SetCursorPosition(0, 0);

                    var maxSourceInstanceLen = _windowWidth - 1 -
                                               (configMetrics.MaxMetricLen +
                                                configMetrics.MaxAggregationTypeLen +
                                                configMetrics.MaxMetricValueLen +
                                                4);

                    foreach (var data in metrics)
                    {
                        var metric = data.metric;
                        var instance = metric.Instance;

                        var siName = Converter.Truncate($"{instance.Source.Name}.{instance.Name}", maxSourceInstanceLen);
                        var metName = Converter.Truncate(metric.Name, configMetrics.MaxMetricLen);
                        var aggType = Converter.Truncate(metric.Aggregation.ToString(),
                            configMetrics.MaxAggregationTypeLen, "");

                        var str = string.Format(
                                $"{{0,-{maxSourceInstanceLen}}} " +
                                $"{{1,-{configMetrics.MaxMetricLen}}} " +
                                $"|{{2,-{configMetrics.MaxAggregationTypeLen}}}|",
                            siName, metName, aggType);

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

                                var valString = value.ToString(data.config.ValueFormat ?? configMetrics.DefaultValueFormat);
                                valString = valString.Length <= configMetrics.MaxMetricValueLen ? valString : valString.Substring(valString.Length - configMetrics.MaxMetricValueLen);
                                Console.Write($"{{0,{configMetrics.MaxMetricValueLen}}}", valString);

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

                if (_config.Common.Mode == TerminalMode.Single)
                    return;

                Task.Delay(_config.Common.RefreshPeriod * 1000).Wait();
            } //while true
        }
    }
}