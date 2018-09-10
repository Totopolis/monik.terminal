using MonikTerminal.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonikTerminal.Enums;
using Newtonsoft.Json.Linq;

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

        public void WriteNewMetricsToConfig()
        {
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

            var json = JsonConvert.SerializeObject(metrics, settings);
            File.WriteAllText("monikNew.json", json);
            Console.WriteLine("New metrics are populated");
        }
    } //end of class
}