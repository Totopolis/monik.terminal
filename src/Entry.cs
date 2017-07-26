using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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
			app.HelpOption("-? | -h | --help");

			var showSources = app.Option("-u | --show-sources", "Display source list", CommandOptionType.NoValue);
			var showLogs = app.Option("-g | --show-logs", "Display logs", CommandOptionType.NoValue);
			var showKeepAlives = app.Option("-k | --show-keepalive", "Display keep-alive statuses", CommandOptionType.NoValue);

			app.OnExecute(() =>
			{
				Config cfg = new Config();

				if (showSources.HasValue())
					cfg.LoadSources().Wait();

				return 0;
			});

			app.Execute(args);
		}
	}
}