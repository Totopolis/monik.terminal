using System.Collections.Generic;
using System.Linq;
using MonikTerminal.Interfaces;
using MonikTerminal.ModelsApp;
using System.Threading.Tasks;
using System;
using MonikTerminal.ModelsApi;
using System.Text;
using MonikTerminal.Enums;

namespace MonikTerminal
{
	public class LogTerminal : ILogTerminal
	{
		private readonly IMonikService _service;
		private readonly IConfig _config;
		private readonly ISourcesCache _sourceCache;

		public LogTerminal(IMonikService aService, IConfig aConfig, ISourcesCache aSourceCache)
		{
			_service = aService;
			_config = aConfig;
			_sourceCache = aSourceCache;
		}

		public void Start()
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
				try
				{
					var task = _service.GetLogs(request);
					task.Wait();

					ELog_[] response = task.Result;

					if (response.Length > 0)
						request.LastID = response.Max(x => x.ID);

					foreach (var log in response)
					{
						var instance = _sourceCache.GetInstance(log.InstanceID);

						var instName = instance.Name.Length <= _config.MaxInstanceLen ? instance.Name : instance.Name.Substring(0, _config.MaxInstanceLen);
						var srcName = instance.Source.Name.Length <= _config.MaxSourceLen ? instance.Source.Name : instance.Source.Name.Substring(0, _config.MaxSourceLen);

						var whenStr = log.Created.ToLocalTime().ToString(_config.TimeTemplate);
						var bodyStr = log.Body.Replace(Environment.NewLine, "");

						var sourceLen = _config.MaxSourceLen + _config.MaxInstanceLen + 1;

						var str = string.Format("{0} {1,-" + sourceLen + "} {2}",
							whenStr,
							srcName + ":" + instName,
							bodyStr);

						str = str.Length <= Console.WindowWidth ? str : str.Substring(0, Console.WindowWidth - 1);

						var sev = (SeverityCutoffType)log.Severity;

						Console.ForegroundColor = ConsoleColor.White;
						Console.BackgroundColor = sev == SeverityCutoffType.Info ? ConsoleColor.Black :
							sev == SeverityCutoffType.Warning ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;

						Console.WriteLine(str);

						Console.ForegroundColor = ConsoleColor.White;
						Console.BackgroundColor = ConsoleColor.Black;
					}
				}
				catch(Exception)
				{
					Console.WriteLine("INTERNAL ERROR");
				}

				Task.Delay(_config.RefreshPeriod * 1000).Wait();
			}
		}

	} //end of class
}