using MonikTerminal.Enums;
using MonikTerminal.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MonikTerminal
{
    public class JsonSerializer : ISerializer
    {
        private readonly IConfig _config;
        private readonly ISourcesCache _cache;

        public JsonSerializer(IConfig config, ISourcesCache cache)
        {
            _config = config;
            _cache = cache;
        }

        public void LoadConfig(string path)
        {
            var jsonConfig = File.ReadAllText(path);
            JsonConvert.PopulateObject(jsonConfig, _config);
            Console.WriteLine("Config loaded");
        }

        public void WriteNewMetricsToConfig(string path)
        {
            try
            {
                JsonConvert.DeserializeObject<MetricConfigArea[]>(_config.MetricsFill.Areas);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Wrong Areas template: {e.Message}");
                return;
            }

            var alreadyInConfig = new HashSet<long>(_config.Metrics.Metrics.Select(m => m.MetricId));
            var data = _cache.Metrics
                .OrderBy(x => x.Instance.Source.Name)
                .ThenBy(x => x.Instance.Name)
                .ThenBy(x => x.Name);

            var metrics =
                from m in data
                where !alreadyInConfig.Contains(m.ID)
                select new
                {
                    MetricId = m.ID,
                    Name = $"{m.Instance.Source.Name}.{m.Instance.Name}.{m.Name}",
                    ValueFormat = m.Aggregation == AggregationType.Gauge
                        ? _config.MetricsFill.ValueFormatGauge
                        : _config.MetricsFill.ValueFormatAccum,
                    Areas = new JRaw(_config.MetricsFill.Areas)
                };

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            var jsonMetrics = JsonConvert.SerializeObject(metrics, settings);
            jsonMetrics = jsonMetrics.Replace("\n", "\n    ");

            const string autoFilledName = nameof(MetricsConfig.MetricsAutoFilled);
            var autoFilledRegex = new Regex($@"(""{autoFilledName}"":)\s*\[((?<BR>\[)|(?<-BR>\])|[^\[\]]*)+\]");

            var jsonConfig = File.ReadAllText(path);

            if (autoFilledRegex.IsMatch(jsonConfig))
            {
                var result = autoFilledRegex.Replace(jsonConfig, $"$1 {jsonMetrics}");
                File.WriteAllText(path, result);
                Console.WriteLine($"New metrics are populated to field {autoFilledName} in {path}");
            }
            else
                Console.WriteLine($"Could not find field {autoFilledName} in config");
        }

    } //end of class
}