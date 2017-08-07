using MonikTerminal.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MonikTerminal
{
	public class MSource
	{
		public int iD { get; set; }
		public string name { get; set; }
		public string description { get; set; }
	}

	public class MInstance
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public int SourceID { get; set; }

		public MSource Source { get; set; }
		public MGroup Grorup { get; set; }
	}

	public class MGroup
	{
		public short ID { get; set; }
		public string Name { get; set; }
		public bool IsDefault { get; set; }

		public int[] Instances { get; set; }

		public List<MInstance> InstanceList { get; set; }
	}

	public class Config : IConfig
	{
		public string ServerUrl { get => "http://url/"; set => throw new NotImplementedException(); }

		public string TimeTemplate { get; set; } = "HH:mm:ss";
		public int MaxSourceLen { get; set; } = 12;
		public int MaxInstanceLen { get; set; } = 8;
		public int RefreshPeriod { get; set; } = 5;
	}

}
