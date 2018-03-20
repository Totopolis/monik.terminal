using System;
using System.Linq;
using System.Threading.Tasks;
using MonikTerminal.Interfaces;
using MonikTerminal.ModelsApi;

namespace MonikTerminal
{
    public class MetricTerminal : IMetricTerminal
    {
        private readonly IMonikService _service;
        private readonly IConfig _config;
        private readonly ISourcesCache _sourceCache;

        public MetricTerminal(IMonikService aService, IConfig aConfig, ISourcesCache aSourceCache)
        {
            _service = aService;
            _config = aConfig;
            _sourceCache = aSourceCache;
        }

        public void Start()
        {
            Console.Title = nameof(MonikTerminal) + ": " + nameof(MetricTerminal);


            while (true)
            {
                try
                {
                    var response = _service.GetMetrics().Result;
                    var metricDescriptions = _service.GetMetricDescriptions().Result.ToDictionary(md => md.Id);

                    response = response
                        .OrderBy(x => _sourceCache.GetInstance(metricDescriptions[x.MetricId].InstanceId).Name)
                        .ThenBy(x => _sourceCache.GetInstance(metricDescriptions[x.MetricId].InstanceId).Source.Name)
                        .ThenBy(x => metricDescriptions[x.MetricId].Name)
                        .ToArray();

                    Console.Clear();

                    foreach (var x in response)
                    {
                        var instance = _sourceCache.GetInstance(metricDescriptions[x.MetricId].InstanceId);
                        
                        var instName    = instance.Name;
                        var srcName     = instance.Source.Name;
                        var metricName  = metricDescriptions[x.MetricId].Name;
                        
                        instName   = instName  .Length <= _config.MaxInstanceLen ? string.Format($"{{0,-{_config.MaxInstanceLen}}}", instName  ) : instName  .Substring(0, _config.MaxInstanceLen - 2) + "..";
                        srcName    = srcName   .Length <= _config.MaxSourceLen   ? string.Format($"{{0,-{_config.MaxSourceLen  }}}", srcName   ) : srcName   .Substring(0, _config.MaxSourceLen   - 2) + "..";
                        metricName = metricName.Length <= _config.MaxMetricName  ? string.Format($"{{0,-{_config.MaxMetricName }}}", metricName) : metricName.Substring(0, _config.MaxMetricName  - 2) + "..";

                        var whenStr = x.Created.ToLocalTime().ToString(_config.TimeTemplate);

                        //var str = string.Format($"{{0,-{_config.MaxSourceLen}}} {{1,-{_config.MaxInstanceLen}}} {{2,-{_config.MaxMetricName}}}",
                        //    srcName,
                        //    instName,
                        //    metricName);

                        var valueStr = string.Format("{0, 5}", x.Value);

                        Console.WriteLine($"{srcName} {instName} {metricName} | {valueStr} | {whenStr}");

                    }//foreach ka
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"INTERNAL ERROR: {ex.Message}");
                }

                if (_config.Mode == TerminalMode.Single)
                    return;

                Task.Delay(_config.RefreshPeriod * 1000).Wait();
            }//while true
        }
    }
}