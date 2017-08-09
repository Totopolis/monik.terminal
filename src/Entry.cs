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
using System.Text;

namespace MonikTerminal
{
	public class Entry
	{
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

			var showSources = app.Option("-u | --show-sources", "Display source list", CommandOptionType.NoValue);
			var showLogs = app.Option("-g | --show-logs", "Display logs", CommandOptionType.NoValue);
			var showKeepAlives = app.Option("-k | --show-keepalive", "Display keep-alive statuses", CommandOptionType.NoValue);

			app.OnExecute(() =>
			{
				var container = Bootstrap.Init();

				var cfg = container.Resolve<IConfig>();
				var service = container.Resolve<IMonikService>();
				var cache = container.Resolve<ISourcesCache>();

				try
				{
					cfg.Load("monik.json");
					cache.Reload().Wait();
				}
				catch(Exception ex)
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

				return 0;
			});

			app.Execute(args);
		}
	}
}