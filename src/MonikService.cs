using System.IO;
using System.Net;
using MonikTerminal.Interfaces;
using MonikTerminal.ModelsApi;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using System.Text;

namespace MonikTerminal
{
	public class MonikService : IMonikService
	{
		private readonly IConfig _cfg;

		public MonikService(IConfig aCfg)
		{
			_cfg = aCfg;
		}

		public async Task<EGroup[]> GetGroups()
		{
			var json = await GetJson("groups");
			var result = JsonConvert.DeserializeObject<EGroup[]>(json);
			return result;
		}

		public async Task<EInstance[]> GetInstances()
		{
			var json = await GetJson("instances");
			var result = JsonConvert.DeserializeObject<EInstance[]>(json);
			return result;
		}

		public async Task<ESource[]> GetSources()
		{
			var json = await GetJson("sources");
			var result = JsonConvert.DeserializeObject<ESource[]>(json);
			return result;
		}

		public async Task<ELog_[]> GetLogs(ELogRequest aRequest)
		{
			var reqJson = JsonConvert.SerializeObject(aRequest);
			var resJson = await PostJson("logs5", reqJson);

			var result = JsonConvert.DeserializeObject<ELog_[]>(resJson);
			return result;
		}

		public async Task<EKeepAlive_[]> GetKeepAlives(EKeepAliveRequest aRequest)
		{
			var reqJson = JsonConvert.SerializeObject(aRequest);
			var resJson = await PostJson("keepalive2", reqJson);

			var result = JsonConvert.DeserializeObject<EKeepAlive_[]>(resJson);
			return result;
		}

		private async Task<string> GetJson(string aMethod)
		{
			var client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

			var stringTask = client.GetStringAsync(_cfg.ServerUrl + aMethod);

			return await stringTask;
		}

		private async Task<string> PostJson(string aMethod, string aJson)
		{
			var client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

			var content = new StringContent(aJson, Encoding.UTF8, "application/json");

			var stringTask = await client.PostAsync(_cfg.ServerUrl + aMethod, content);

			// TODO: check resulted http status code

			return await stringTask.Content.ReadAsStringAsync();
		}
	}
}