using MonikTerminal.Interfaces;
using System;
using System.Linq;

namespace MonikTerminal
{
    public class MetricTerminal : BaseTerminal, IMetricTerminal
    {
        public MetricTerminal(IMonikService aService, IConfig aConfig, ISourcesCache aSourceCache)
            : base(aService, aConfig, aSourceCache)
        {
        }

        protected MetricsConfig ConfigMetrics => Config.Metrics;

        protected override void OnStart()
        {
            Console.Clear();
            Console.Title =
                $"{nameof(MonikTerminal)}: {nameof(MetricTerminal)}";
        }

        protected override void Show()
        {
            var windows = Service.GetMetricsWindow().Result;
            var metrics =
                from config in ConfigMetrics.Metrics
                join metric in SourceCache.Metrics on config.MetricId equals metric.ID
                join window in windows on metric.ID equals window.MetricId
                select new {metric, window, config};

            SoftClear();

            var maxSourceInstanceLen = Console.WindowWidth - 1 -
                                       (ConfigMetrics.MaxMetricLen +
                                        ConfigMetrics.MaxAggregationTypeLen +
                                        ConfigMetrics.MaxMetricValueLen +
                                        4);

            foreach (var data in metrics)
            {
                var metric = data.metric;
                var instance = metric.Instance;

                var siName = Converter.Truncate($"{instance.Source.Name}.{instance.Name}", maxSourceInstanceLen);
                var metName = Converter.Truncate(metric.Name, ConfigMetrics.MaxMetricLen);
                var aggType = Converter.Truncate(metric.Aggregation.ToString(),
                    ConfigMetrics.MaxAggregationTypeLen, "");

                var str = string.Format(
                    $"{{0,-{maxSourceInstanceLen}}} " +
                    $"{{1,-{ConfigMetrics.MaxMetricLen}}} " +
                    $"|{{2,-{ConfigMetrics.MaxAggregationTypeLen}}}|",
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

                        var valString = value.ToString(data.config.ValueFormat ?? ConfigMetrics.DefaultValueFormat);
                        valString = valString.Length <= ConfigMetrics.MaxMetricValueLen
                            ? valString
                            : valString.Substring(valString.Length - ConfigMetrics.MaxMetricValueLen);
                        Console.Write($"{{0,{ConfigMetrics.MaxMetricValueLen}}}", valString);

                        Console.BackgroundColor = ConsoleColor.Black;
                        break;
                    }
                }

                Console.WriteLine();
            }
        }
    }
}