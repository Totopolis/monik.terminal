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
			var request = new EKeepAliveRequest();

		    Console.Title = nameof(MonikTerminal) + ": " + nameof(KeepAliveTerminal);
                
            while (true)
			{
				try
				{
					var task = _service.GetKeepAlives(request);
					task.Wait();

					EKeepAlive_[] response = task.Result;
					response = response.
					    GroupBy(x => _sourceCache.GetInstance(x.InstanceID).Name).OrderBy(g=>g.Key).
					    SelectMany(g=>g.OrderBy(x=> _sourceCache.GetInstance(x.InstanceID).Source.Name)).ToArray();

					Console.Clear();

					foreach (var ka in response)
					{
						var instance = _sourceCache.GetInstance(ka.InstanceID);

						var instName = instance.Name.Length <= _config.MaxInstanceLen ? instance.Name : instance.Name.Substring(0, _config.MaxInstanceLen-2)+"..";
						var srcName = instance.Source.Name.Length <= _config.MaxSourceLen ? instance.Source.Name : instance.Source.Name.Substring(0, _config.MaxSourceLen-2)+"..";

						var whenStr = ka.Created.ToLocalTime().ToString(_config.TimeTemplate);

						var sourceLen = _config.MaxSourceLen + _config.MaxInstanceLen + 1;

						var str = string.Format($"{{0,-{_config.MaxSourceLen}}} {{1,-{_config.MaxInstanceLen}}} | ",
							srcName,
							instName);

						Console.Write(str);

						if ((DateTime.Now - ka.Created.ToLocalTime()).TotalSeconds > _config.KeepAliveWarnSeconds)
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

				if (_config.Mode == TerminalMode.Single)
					return;

				Task.Delay(_config.RefreshPeriod * 1000).Wait();
			}//while true
		}

	} //end of class
}