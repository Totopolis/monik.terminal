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
	}

	public class MGroup
	{
		public short ID { get; set; }
		public string Name { get; set; }
		public bool IsDefault { get; set; }

		public int[] Instances { get; set; }
	}

	public class Config
	{
		public string UrlPrefix { get; set; } = "http://url/";

		public async Task LoadSources()
		{
			// 1. Download
			string _sources = await RequestJson("sources");
			Console.WriteLine("Sources downloaded");

			string _instances = await RequestJson("instances");
			Console.WriteLine("Instances downloaded");

			string _groups = await RequestJson("groups");
			Console.WriteLine("Groups downloaded");

			// 2. Parse
			var srcList = JsonConvert.DeserializeObject<MSource[]>(_sources);
			var insList = JsonConvert.DeserializeObject<MInstance[]>(_instances);
			var grList = JsonConvert.DeserializeObject<MGroup[]>(_groups);
		}

		private async Task<string> RequestJson(string aUrlPostfix)
		{
			var client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
			client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

			var stringTask = client.GetStringAsync(UrlPrefix + aUrlPostfix);

			return await stringTask;
		}
	}

}
