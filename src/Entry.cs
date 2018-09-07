using Autofac;
using Microsoft.Extensions.CommandLineUtils;
using MonikTerminal.Enums;
using MonikTerminal.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MonikTerminal
{
    public class Entry
	{
	    private const string DefaultConfigPath = "monik.json";

        public static void Point(string[] args)
		{
			//Console.ForegroundColor = ConsoleColor.White;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.OutputEncoding = Encoding.UTF8;

			// Program.exe <-g|--greeting|-$ <greeting>> [name <fullname>]
			// [-?|-h|--help] [-u|--uppercase]
			var app = new CommandLineApplication(throwOnUnexpectedArg: false);
			app.Name = "monik";
			var help = app.HelpOption("-? | -h | --help");

			var showSources    = app.Option("-u | --show-sources",    "Display source list",         CommandOptionType.NoValue);
			var showLogs       = app.Option("-g | --show-logs",       "Display logs",                CommandOptionType.NoValue);
			var showKeepAlives = app.Option("-k | --show-keepalive",  "Display keep-alive statuses", CommandOptionType.NoValue);
            var listMetrics    = app.Option("-l | --list-metrics",    "Display all metrics",         CommandOptionType.NoValue);
            var fillMetrics    = app.Option("-f | --fill-metrics",    "Fill config with metrics",    CommandOptionType.NoValue);
            var showMetrics    = app.Option("-m | --show-metrics",    "Display metrics Values",      CommandOptionType.NoValue);
			var customConfig   = app.Option("-c | --custom-settings", "Use custom settings",         CommandOptionType.SingleValue);

			app.OnExecute(() =>
			{
				var container = Bootstrap.Init();

				var cfg = container.Resolve<IConfig>();
				var service = container.Resolve<IMonikService>();
				var cache = container.Resolve<ISourcesCache>();

			    var configPath = customConfig.HasValue()
			        ? customConfig.Value()
                    : DefaultConfigPath;

				try
				{
				    var jsonConfig = File.ReadAllText(configPath);
				    JsonConvert.PopulateObject(jsonConfig, cfg);
				    Console.WriteLine("Config loaded");

					cache.Reload().Wait();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					return -1;
				}

				Console.WriteLine();

				if (showSources.HasValue())
				{
					foreach (var gr in cache.Groups)
					{
						Console.WriteLine($"Group: {gr.Name}");

						foreach (var inst in gr.Instances)
							Console.WriteLine($"  {inst.Source.Name}::{inst.Name}");
					}
				}

				if (showLogs.HasValue())
				{
					var term = container.Resolve<ILogTerminal>();
					term.Start();
				}

				if (showKeepAlives.HasValue())
				{
				    var term = container.Resolve<IKeepAliveTerminal>();
					term.Start();
				}

				if (showMetrics.HasValue())
				{
				    var term = container.Resolve<IMetricTerminal>();
					term.Start();
				}

			    if (listMetrics.HasValue())
			    {
			        var (maxSource, maxName) = cache.Metrics.Aggregate((maxSource: 0, maxName: 0), (a, b) =>
			        {
			            a.maxSource = Math.Max(a.maxSource, b.Instance.Source.Name.Length + b.Instance.Name.Length + 1);
			            a.maxName = Math.Max(a.maxName, b.Name.Length);
			            return a;
			        });
			        var data = cache.Metrics.OrderBy(m => m.ID);
                    foreach (var metric in data)
			        {
                        Console.WriteLine($"{{0}}\t{{1,-{maxName}}} {{2,-{maxSource}}} {{3}}",
                            metric.ID, metric.Name,
                            metric.Instance.Source.Name + "." + metric.Instance.Name,
                            metric.Aggregation);
			        }
			    }

			    if (fillMetrics.HasValue())
			    {
			        var alreadyInConfig = new HashSet<long>(cfg.Metrics.Metrics.Select(m => m.MetricId));
			        var data = cache.Metrics
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
			                    ? cfg.MetricsFill.ValueFormatGauge
			                    : cfg.MetricsFill.ValueFormatAccum,
			                Areas = new JRaw(cfg.MetricsFill.Areas)
			            };

			        var settings = new JsonSerializerSettings
			        {
			            Formatting = Formatting.Indented,
			            DefaultValueHandling = DefaultValueHandling.Ignore,
			            NullValueHandling = NullValueHandling.Ignore
			        };

			        var json = JsonConvert.SerializeObject(metrics, settings);
			        File.WriteAllText("monikNew.json", json);
			    }

				return 0;
			});

			app.Execute(args);

		    Console.ReadKey(true);
		}
	}
}