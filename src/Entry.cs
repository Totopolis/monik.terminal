using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using Autofac;
using MonikTerminal.Interfaces;
using MonikTerminal.ModelsApi;
using System.Linq;

namespace MonikTerminal
{
	public class Entry
	{
		public static void Point(string[] args)
		{
			// Program.exe <-g|--greeting|-$ <greeting>> [name <fullname>]
			// [-?|-h|--help] [-u|--uppercase]
			var app = new CommandLineApplication(throwOnUnexpectedArg: false);
			app.Name = "monik";
			var help = app.HelpOption("-? | -h | --help");

			var showSources = app.Option("-u | --show-sources", "Display source list", CommandOptionType.NoValue);
			var showLogs = app.Option("-g | --show-logs", "Display logs", CommandOptionType.NoValue);
			var showKeepAlives = app.Option("-k | --show-keepalive", "Display keep-alive statuses", CommandOptionType.NoValue);

			app.OnExecute(() =>
			{
				var container = Bootstrap.Init();

				var service = container.Resolve<IMonikService>();
				var cache = container.Resolve<ISourcesCache>();
				cache.Reload().Wait();

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
					var request = new ELogRequest()
					{
						LastID = null,
						Top = 50,
						Level = null,
						SeverityCutoff = 30
					};

					while (true)
					{
						var task = service.GetLogs(request);
						task.Wait();

						Console.WriteLine("123");

						ELog_[] response = task.Result;

						if (response.Length > 0)
							request.LastID = response.Max(x => x.ID);

						foreach (var log in response)
						{
							Console.WriteLine(log.Body);
						}

						Task.Delay(5000);
					}
				}

				return 0;
			});

			app.Execute(args);
		}
	}
}