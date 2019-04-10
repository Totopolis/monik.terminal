using MonikTerminal.Interfaces;
using MonikTerminal.ModelsApi;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MonikTerminal
{
    public class MonikService : IMonikService
    {
        private static readonly TimeSpan LoadTimeout = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);

		private readonly IConfig _cfg;

		public MonikService(IConfig aCfg)
		{
			_cfg = aCfg;
		}

		public async Task<EGroup[]> GetGroups()
		{
			var json = await GetJson("groups", LoadTimeout);
			var result = JsonConvert.DeserializeObject<EGroup[]>(json);
			return result;
		}

		public async Task<EInstance[]> GetInstances()
		{
			var json = await GetJson("instances", LoadTimeout);
			var result = JsonConvert.DeserializeObject<EInstance[]>(json);
			return result;
		}

		public async Task<ESource[]> GetSources()
		{
			var json = await GetJson("sources", LoadTimeout);
			var result = JsonConvert.DeserializeObject<ESource[]>(json);
			return result;
		}

		public async Task<ELog_[]> GetLogs(ELogRequest aRequest)
		{
			var reqJson = JsonConvert.SerializeObject(aRequest);
			var resJson = await PostJson("logs5", reqJson, RequestTimeout);

			var result = JsonConvert.DeserializeObject<ELog_[]>(resJson);
			return result;
		}

		public async Task<EKeepAlive_[]> GetKeepAlives(EKeepAliveRequest aRequest)
		{
			var reqJson = JsonConvert.SerializeObject(aRequest);
			var resJson = await PostJson("keepalive2", reqJson, RequestTimeout);

			var result = JsonConvert.DeserializeObject<EKeepAlive_[]>(resJson);
			return result;
		}

	    public async Task<EMetricWindow[]> GetMetricsWindow()
	    {
	        var json = await GetJson("metrics/windows", RequestTimeout);
	        var result = JsonConvert.DeserializeObject<EMetricWindow[]>(json);
	        return result;
        }

	    public async Task<EMetric[]> GetMetrics()
	    {
	        var json = await GetJson("metrics", RequestTimeout);
	        var result = JsonConvert.DeserializeObject<EMetric[]>(json);
	        return result;
	    }


        private async Task<string> GetJson(string aMethod, TimeSpan timeout)
		{
            var client = new HttpClient {Timeout = timeout};
            client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

			var stringTask = client.GetStringAsync(_cfg.Common.ServerUrl + aMethod);

			return await stringTask;
		}

		private async Task<string> PostJson(string aMethod, string aJson, TimeSpan timeout)
		{
            var client = new HttpClient {Timeout = timeout};
            client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

			var content = new StringContent(aJson, Encoding.UTF8, "application/json");

			var stringTask = await client.PostAsync(_cfg.Common.ServerUrl + aMethod, content);

			// TODO: check resulted http status code

			return await stringTask.Content.ReadAsStringAsync();
		}
	}
}