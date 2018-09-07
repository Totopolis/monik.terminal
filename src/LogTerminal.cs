using MonikTerminal.Enums;
using MonikTerminal.Interfaces;
using MonikTerminal.ModelsApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
		    var configLog = _config.Log;

			var request = new ELogRequest()
			{
				LastID = null,
				Top = configLog.Top,
				Level = configLog.LevelFilter == LevelType.None ? null : (byte?)configLog.LevelFilter,
				SeverityCutoff = configLog.SeverityCutoff == SeverityCutoffType.None ? null : (byte?)configLog.SeverityCutoff
			};

		    Console.Title =
		        $"{nameof(MonikTerminal)}: {nameof(LogTerminal)}{(configLog.LevelFilter != LevelType.None ? $"({Enum.GetName(typeof(LevelType), configLog.LevelFilter)})" : "")}";

            while (true)
			{
				try
                {
                    var task = _service.GetLogs(request);
                    task.Wait();

                    ELog_[] response = task.Result;

                    if (response.Length > 0)
                        request.LastID = response.Max(x => x.ID);
                    response = GroupDuplicatingLogs(response).OrderBy(l=>l.ID).ToArray();

                    foreach (var log in response)
                    {
                        var instance = _sourceCache.GetInstance(log.InstanceID);

                        var instName = Converter.Truncate(instance.Name, configLog.MaxInstanceLen);
                        var srcName  = Converter.Truncate(instance.Source.Name, configLog.MaxSourceLen);

                        var whenStr = log.Created.ToLocalTime().ToString(log.Doubled ? configLog.DoubledTimeTemplate : _config.Common.TimeTemplate);

                        var bodyStr = log.Body.Replace(Environment.NewLine, "");

                        var sev = (SeverityCutoffType)log.Severity;
                        var level = (LevelType)log.Level;

                        // TODO: support ShowLevelVerbose

                        var str = string.Format("{0} {1,-" + configLog.MaxSourceLen + "} {2,-" + configLog.MaxInstanceLen + "} {3} {4} | {5}",
                            whenStr,
                            srcName,
                            instName,
                            Converter.LevelTypeToString(level),
                            Converter.SeverityToString(sev),
                            bodyStr);

                        str = str.Length <= Console.WindowWidth ? str : str.Substring(0, Console.WindowWidth - 1);

                        Console.BackgroundColor = sev >= SeverityCutoffType.Info ? ConsoleColor.Black :
                            sev == SeverityCutoffType.Warning ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;

                        Console.Write(str);

                        Console.BackgroundColor = ConsoleColor.Black;

                        Console.WriteLine();
                    }
                }
                catch (Exception ex)
				{
					Console.WriteLine($"{DateTime.Now.ToString(_config.Common.TimeTemplate)} INTERNAL ERROR: {ex.Message}");
				}

				if (_config.Common.Mode == TerminalMode.Single)
					return;

				Task.Delay(_config.Common.RefreshPeriod * 1000).Wait();
			}
		}

	    public IEnumerable<ELog_> GroupDuplicatingLogs(ELog_[] response)
        {
            var result = response?.GroupBy(r => new { r.InstanceID, r.Body, r.Severity, r.Level }).SelectMany(g =>
            {
                var min = g.Min(r => r.Created);
                var firstIn5Sec = g.GroupBy(r =>
                    {
                        var totalSeconds = (r.Created - min).TotalSeconds;
                        var rez = totalSeconds - totalSeconds % _config.Common.RefreshPeriod;

                        return rez;
                    })
                    .Select(inner =>
                    {
                        var eLog = inner.First();
                        if (inner.Count() > 1)
                            eLog.Doubled = true;
                        return eLog;
                    }).ToArray();

                return firstIn5Sec;
            });
            return result;
        }
    } //end of class
}