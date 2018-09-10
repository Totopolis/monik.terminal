using MonikTerminal.Enums;
using MonikTerminal.Interfaces;
using MonikTerminal.ModelsApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonikTerminal
{
    public class LogTerminal : BaseTerminal, ILogTerminal
	{
	    private ELogRequest _request;

		public LogTerminal(IMonikService aService, IConfig aConfig, ISourcesCache aSourceCache)
		    : base(aService, aConfig, aSourceCache)
        {
		}

	    protected LogConfig ConfigLog => Config.Log;

	    protected override void OnStart()
	    {
	        _request = new ELogRequest()
	        {
	            LastID = null,
	            Top = ConfigLog.Top,
	            Level = ConfigLog.LevelFilter == LevelType.None ? null : (byte?)ConfigLog.LevelFilter,
	            SeverityCutoff = ConfigLog.SeverityCutoff == SeverityCutoffType.None ? null : (byte?)ConfigLog.SeverityCutoff
	        };

	        Console.Title =
	            $"{nameof(MonikTerminal)}: {nameof(LogTerminal)}{(ConfigLog.LevelFilter != LevelType.None ? $"({Enum.GetName(typeof(LevelType), ConfigLog.LevelFilter)})" : "")}";
        }

        protected override void Show()
	    {
            var task = Service.GetLogs(_request);
            task.Wait();

            ELog_[] response = task.Result;

            if (response.Length > 0)
                _request.LastID = response.Max(x => x.ID);
            response = GroupDuplicatingLogs(response).OrderBy(l => l.ID).ToArray();

            foreach (var log in response)
            {
                var instance = SourceCache.GetInstance(log.InstanceID);

                var instName = Converter.Truncate(instance.Name, ConfigLog.MaxInstanceLen);
                var srcName = Converter.Truncate(instance.Source.Name, ConfigLog.MaxSourceLen);

                var whenStr = log.Created.ToLocalTime().ToString(log.Doubled ? ConfigLog.DoubledTimeTemplate : ConfigCommon.TimeTemplate);

                var bodyStr = log.Body.Replace(Environment.NewLine, "");

                var sev = (SeverityCutoffType)log.Severity;
                var level = (LevelType)log.Level;

                // TODO: support ShowLevelVerbose

                var str = string.Format("{0} {1,-" + ConfigLog.MaxSourceLen + "} {2,-" + ConfigLog.MaxInstanceLen + "} {3} {4} | {5}",
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

	    public IEnumerable<ELog_> GroupDuplicatingLogs(ELog_[] response)
        {
            var result = response?.GroupBy(r => new { r.InstanceID, r.Body, r.Severity, r.Level }).SelectMany(g =>
            {
                var min = g.Min(r => r.Created);
                var firstIn5Sec = g.GroupBy(r =>
                    {
                        var totalSeconds = (r.Created - min).TotalSeconds;
                        var rez = totalSeconds - totalSeconds % ConfigCommon.RefreshPeriod;

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