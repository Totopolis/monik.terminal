using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonikTerminal.Interfaces;
using MonikTerminal.ModelsApi;

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

        public void Start()
        {
            Console.Title =
                $"{nameof(MonikTerminal)}: {nameof(MetricTerminal)}({_config.MetricTerminalMode} mode{(_config.MetricTerminalMode == MetricTerminalMode.TimeWindow ? $", {_config.MetricAggWindow5MinWidth * 5} min" : "")})";

            while (true)
            {
                try
                {
                    EMetric_[] response;
                    Dictionary<long, EMetricDescription_> metricDescriptions;

                    if (_config.MetricTerminalMode == MetricTerminalMode.Current)
                    {
                        response           = _service.GetCurrentMetrics().Result;
                        metricDescriptions = _service.GetMetricDescriptions().Result.ToDictionary(md => md.Id);

                    }
                    else
                    {
                        metricDescriptions = _service.GetMetricDescriptions().Result.ToDictionary(md => md.Id);

                        response = metricDescriptions.Select(md =>
                                                      {
                                                          EMetric_ rez;
                                                          var      tmp = _service.GetHistoryMetrics(new EMetricHistoryRequest() {MetricId = md.Key, Count = _config.MetricAggWindow5MinWidth}).Result;

                                                          if (md.Value.Type == MetricType.Accumulator)
                                                          {
                                                              rez = new EMetric_
                                                              {
                                                                  Value    = tmp.Sum(v => v.Value),
                                                                  Created  = tmp.Max(v => v.Created),
                                                                  MetricId = md.Key
                                                              };
                                                          }
                                                          else
                                                          {
                                                              rez = new EMetric_
                                                              {
                                                                  Value    = tmp.Length > 0 ? tmp.Sum(v => v.Value) / tmp.Length : 0,
                                                                  Created  = tmp.Max(v => v.Created),
                                                                  MetricId = md.Key
                                                              };
                                                          }

                                                          return rez;
                                                      })
                                                     .ToArray();
                    }

                        response = response
                                  .OrderBy(x => _sourceCache.GetInstance(metricDescriptions[x.MetricId].InstanceId).Name)
                                  .ThenBy(x => _sourceCache.GetInstance(metricDescriptions[x.MetricId].InstanceId).Source.Name)
                                  .ThenBy(x => metricDescriptions[x.MetricId].Name)
                                  .ToArray();

                    Console.Clear();

                    foreach (var x in response)
                    {
                        var instance = _sourceCache.GetInstance(metricDescriptions[x.MetricId].InstanceId);

                        var instName   = instance.Name;
                        var srcName    = instance.Source.Name;
                        var metricName = metricDescriptions[x.MetricId].Name;

                        instName = instName.Length <= _config.MaxInstanceLen ? string.Format($"{{0,-{_config.MaxInstanceLen}}}", instName) : instName.Substring(0, _config.MaxInstanceLen - 2) + "..";
                        srcName  = srcName.Length  <= _config.MaxSourceLen   ? string.Format($"{{0,-{_config.MaxSourceLen}}}",    srcName) : srcName .Substring(0, _config.MaxSourceLen   - 2) + "..";

                        metricName = metricName.Length <= _config.MaxMetricName
                            ? string.Format($"{{0,-{_config.MaxMetricName}}}", metricName)
                            : metricName.Substring(0, _config.MaxMetricName - 2) + "..";
                        
                        //var str = string.Format($"{{0,-{_config.MaxSourceLen}}} {{1,-{_config.MaxInstanceLen}}} {{2,-{_config.MaxMetricName}}}",
                        //    srcName,
                        //    instName,
                        //    metricName);

                        var valueStr = string.Format("{0, 5}", x.Value);

                        Console.WriteLine($"{srcName} {instName} {metricName} | {valueStr}");
                    } //foreach ka
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