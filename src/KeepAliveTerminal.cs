using MonikTerminal.Interfaces;
using MonikTerminal.ModelsApi;
using System;
using System.Linq;

namespace MonikTerminal
{
    public class KeepAliveTerminal : BaseTerminal, IKeepAliveTerminal
	{
	    private EKeepAliveRequest _request;

	    public KeepAliveTerminal(IMonikService aService, IConfig aConfig, ISourcesCache aSourceCache)
	        : base(aService, aConfig, aSourceCache)
	    {
	    }

	    protected KeepAliveConfig ConfigKeepAlive => Config.KeepAlive;

	    protected override void OnStart()
	    {
	        _request = new EKeepAliveRequest();

	        Console.Title = nameof(MonikTerminal) + ": " + nameof(KeepAliveTerminal);
        }

	    protected override void Show()
	    {
	        var task = Service.GetKeepAlives(_request);
	        task.Wait();

	        EKeepAlive_[] response = task.Result;
	        response = response
	            .OrderBy(x => SourceCache.GetInstance(x.InstanceID).Name)
	            .ThenBy(x => SourceCache.GetInstance(x.InstanceID).Source.Name)
	            .ToArray();

	        Console.Clear();

	        foreach (var ka in response)
	        {
	            var instance = SourceCache.GetInstance(ka.InstanceID);

	            var instName = Converter.Truncate(instance.Name, ConfigKeepAlive.MaxInstanceLen);
	            var srcName = Converter.Truncate(instance.Source.Name, ConfigKeepAlive.MaxSourceLen);

	            var whenStr = ka.Created.ToLocalTime().ToString(ConfigCommon.TimeTemplate);

	            var str = string.Format($"{{0,-{ConfigKeepAlive.MaxSourceLen}}} {{1,-{ConfigKeepAlive.MaxInstanceLen}}} | ",
	                srcName,
	                instName);

	            Console.Write(str);

	            if ((DateTime.Now - ka.Created.ToLocalTime()).TotalSeconds > ConfigKeepAlive.KeepAliveWarnSeconds)
	            {
                    WriteWithColor("[ERROR]", ConsoleColor.DarkRed);
	            }
	            else
	            {
	                WriteWithColor("[ OK  ]", ConsoleColor.DarkGreen);
	            }

	            Console.WriteLine(" | " + whenStr);

	        }//foreach ka
        }

	} //end of class
}