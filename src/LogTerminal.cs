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
				Top = _config.Top,
				Level = _config.LevelFilter == LevelType.None ? null : (byte?)_config.LevelFilter,
				SeverityCutoff = _config.SeverityCutoff == SeverityCutoffType.None ? null : (byte?)_config.SeverityCutoff
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
                    response = GroupDuplicatingLogs(response);

                    foreach (var log in response)
                    {
                        var instance = _sourceCache.GetInstance(log.InstanceID);

                        var instName = instance.Name.Length <= _config.MaxInstanceLen ? instance.Name : instance.Name.Substring(0, _config.MaxInstanceLen - 2) + "..";
                        var srcName = instance.Source.Name.Length <= _config.MaxSourceLen ? instance.Source.Name : instance.Source.Name.Substring(0, _config.MaxSourceLen - 2) + "..";

                        var whenStr = log.Created.ToLocalTime().ToString(_config.TimeTemplate);
                        if (log.Doubled)
                            whenStr = log.Created.ToLocalTime().ToString(_config.DoubledTimeTemplate);
                        var bodyStr = log.Body.Replace(Environment.NewLine, "");

                        var sev = (SeverityCutoffType)log.Severity;
                        var level = (LevelType)log.Level;

                        // TODO: support ShowLevelVerbose

                        var str = string.Format("{0} {1,-" + _config.MaxSourceLen + "} {2,-" + _config.MaxInstanceLen + "} {3} {4} | {5}",
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
					Console.WriteLine("INTERNAL ERROR: " + ex.Message);
				}

				if (_config.Mode == TerminalMode.Single)
					return;

				Task.Delay(_config.RefreshPeriod * 1000).Wait();
			}
		}

	    public ELog_[] GroupDuplicatingLogs(ELog_[] response)
        {
            response = response?.GroupBy(r => new { r.InstanceID, r.Body, r.Severity, r.Level }).SelectMany(g =>
            {
                var min = g.Min(r => r.Created);
                var firstIn5Sec = g.GroupBy(r =>
                    {
                        var totalSeconds = (r.Created - min).TotalSeconds;
                        var rez = totalSeconds - totalSeconds % _config.RefreshPeriod;

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
            }).ToArray();
            return response;
        }
    } //end of class
}