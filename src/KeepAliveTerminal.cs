using MonikTerminal.Interfaces;
using MonikTerminal.ModelsApi;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonikTerminal
{
	public class KeepAliveTerminal : IKeepAliveTerminal
	{
		private readonly IMonikService _service;
		private readonly IConfig _config;
		private readonly ISourcesCache _sourceCache;

		public KeepAliveTerminal(IMonikService aService, IConfig aConfig, ISourcesCache aSourceCache)
		{
			_service = aService;
			_config = aConfig;
			_sourceCache = aSourceCache;
		}

		public void Start()
		{
		    var configKeepAlive = _config.KeepAlive;

			var request = new EKeepAliveRequest();

		    Console.Title = nameof(MonikTerminal) + ": " + nameof(KeepAliveTerminal);
                
            while (true)
			{
				try
				{
					var task = _service.GetKeepAlives(request);
					task.Wait();

					EKeepAlive_[] response = task.Result;
				    response = response
				        .OrderBy(x => _sourceCache.GetInstance(x.InstanceID).Name)
				        .ThenBy(x => _sourceCache.GetInstance(x.InstanceID).Source.Name)
				        .ToArray();

					Console.Clear();

					foreach (var ka in response)
					{
						var instance = _sourceCache.GetInstance(ka.InstanceID);

						var instName = Converter.Truncate(instance.Name, configKeepAlive.MaxInstanceLen);
						var srcName = Converter.Truncate(instance.Source.Name, configKeepAlive.MaxSourceLen);

						var whenStr = ka.Created.ToLocalTime().ToString(_config.Common.TimeTemplate);

						var str = string.Format($"{{0,-{configKeepAlive.MaxSourceLen}}} {{1,-{configKeepAlive.MaxInstanceLen}}} | ",
							srcName,
							instName);

						Console.Write(str);

						if ((DateTime.Now - ka.Created.ToLocalTime()).TotalSeconds > configKeepAlive.KeepAliveWarnSeconds)
						{
							Console.BackgroundColor = ConsoleColor.DarkRed;
							Console.Write("[ERROR]");
							Console.BackgroundColor = ConsoleColor.Black;
						}
						else
						{
							Console.BackgroundColor = ConsoleColor.DarkGreen;
							Console.Write("[ OK  ]");
							Console.BackgroundColor = ConsoleColor.Black;
						}

						Console.WriteLine(" | " + whenStr);

					}//foreach ka
				}
				catch(Exception ex)
				{
					Console.WriteLine("INTERNAL ERROR: " + ex.Message);
				}

				if (_config.Common.Mode == TerminalMode.Single)
					return;

				Task.Delay(_config.Common.RefreshPeriod * 1000).Wait();
			}//while true
		}

	} //end of class
}