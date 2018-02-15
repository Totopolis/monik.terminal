using MonikTerminal.Enums;
using MonikTerminal.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MonikTerminal
{
	//public class MSource
	//{
	//	public int iD { get; set; }
	//	public string name { get; set; }
	//	public string description { get; set; }
	//}

	//public class MInstance
	//{
	//	public int ID { get; set; }
	//	public string Name { get; set; }
	//	public int SourceID { get; set; }

	//	public MSource Source { get; set; }
	//	public MGroup Grorup { get; set; }
	//}

	//public class MGroup
	//{
	//	public short ID { get; set; }
	//	public string Name { get; set; }
	//	public bool IsDefault { get; set; }

	//	public int[] Instances { get; set; }

	//	public List<MInstance> InstanceList { get; set; }
	//}

	public enum TerminalMode
	{
		Stream,
		Single
	}

	public class Config : IConfig
	{
		public string ServerUrl { get; set; } = "http://url/";

		public string             TimeTemplate         { get; set; } = "HH:mm";
	    public string             DoubledTimeTemplate  { get; set; } = "HH:**";
        public int                MaxSourceLen         { get; set; } = 12;
		public int                MaxInstanceLen       { get; set; } = 8;
		public int                RefreshPeriod        { get; set; } = 5;
		public int                KeepAliveWarnSeconds { get; set; } = 60;
		public LevelType          LevelFilter          { get; set; } = LevelType.None;
		public SeverityCutoffType SeverityCutoff       { get; set; } = SeverityCutoffType.None;
		public bool               ShowLevelVerbose     { get; set; } = true;
		public int                Top                  { get; set; } = 25;
		public TerminalMode       Mode                 { get; set; } = TerminalMode.Stream;

		public void Load(string aFileName)
		{
			string json = File.ReadAllText(aFileName);

			var jobj = JObject.Parse(json);
			IDictionary<string, JToken> dic = jobj;

			if (dic.ContainsKey("ServerUrl"))
				ServerUrl = (string)dic["ServerUrl"];

			if (dic.ContainsKey("TimeTemplate"))
				TimeTemplate = (string)dic["TimeTemplate"];

			if (dic.ContainsKey("DoubledTimeTemplate"))
			    DoubledTimeTemplate = (string)dic["DoubledTimeTemplate"];

			if (dic.ContainsKey("MaxSourceLen"))
				MaxSourceLen = (int)dic["MaxSourceLen"];

			if (dic.ContainsKey("MaxInstanceLen"))
				MaxInstanceLen = (int)dic["MaxInstanceLen"];

			if (dic.ContainsKey("RefreshPeriod"))
				RefreshPeriod = (int)dic["RefreshPeriod"];

			if (dic.ContainsKey("KeepAliveWarnSeconds"))
				KeepAliveWarnSeconds = (int)dic["KeepAliveWarnSeconds"];

			if (dic.ContainsKey("LevelFilter"))
				LevelFilter = Converter.StringToLevelType((string)dic["LevelFilter"]);

			if (dic.ContainsKey("SeverityCutoff"))
				SeverityCutoff = Converter.StringToSeverity((string)dic["SeverityCutoff"]);

			if (dic.ContainsKey("ShowLevelVerbose"))
				ShowLevelVerbose = (bool)dic["ShowLevelVerbose"];

			if (dic.ContainsKey("Top"))
				Top = (int)dic["Top"];

			if (dic.ContainsKey("Mode"))
			{
				string md = (string)dic["Mode"];
				Mode = md == "single" ? TerminalMode.Single : TerminalMode.Stream;
			}

			Console.WriteLine("Config loaded");
		}
	}
}
