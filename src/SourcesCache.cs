using System.Collections.Generic;
using System.Linq;
using MonikTerminal.Interfaces;
using MonikTerminal.ModelsApp;
using System.Threading.Tasks;
using System;

namespace MonikTerminal
{
	public class SourcesCache : ISourcesCache
	{
		private readonly IMonikService _service;
		private List<Group> _groups;
		private List<Source> _sources;
		private Dictionary<int, Instance> _instances;
	    private List<Metric> _metrics;

		private readonly Source _unknownSource;
		private readonly Instance _unknownInstance;

		public SourcesCache(IMonikService aService)
		{
			_service = aService;

			_unknownSource = new Source {ID = -1, Name = "_UNKNOWN_"};
			_unknownInstance = new Instance {ID = -1, Name = "_UNKNOWN_", Source = _unknownSource};
		}

		public async Task Reload()
		{
			var sources = await _service.GetSources();
			Console.WriteLine("Sources downloaded");

			var instances = await _service.GetInstances();
			Console.WriteLine("Instances downloaded");

		    var metrics = await _service.GetMetrics();
		    Console.WriteLine("Metrics are downloaded");

            var groups = await _service.GetGroups();
			Console.WriteLine("Groups downloaded");

			_sources = sources.Select(x => new Source
			{
				ID = x.ID,
				Name = x.Name
			}).ToList();

			_instances = new Dictionary<int, Instance>();
			foreach (var it in instances)
			{
				var src = _sources.FirstOrDefault(x => x.ID == it.SourceID) ?? _unknownSource;

				var instance = new Instance
				{
					ID = it.ID,
					Name = it.Name,
					Source = src
				};

				_instances.Add(instance.ID, instance);
			}

		    _metrics = metrics.Select(x => new Metric
		    {
		        ID = x.ID,
		        Name = x.Name,
		        Aggregation = x.Aggregation,
		        Instance = GetInstance(x.InstanceID)
		    }).ToList();

			_groups = new List<Group>();
			foreach (var it in groups)
			{
				var gr = new Group
				{
					ID = it.ID,
					IsDefault = it.IsDefault,
					Name = it.Name,
					Instances = it.Instances
						.Where(v => _instances.Keys.Count(x => x == v) > 0)
						.Select(v => _instances.Values.First(x => x.ID == v)).ToList()
				};

				_groups.Add(gr);
			}
		}

		public Group[] Groups => _groups.ToArray();

		public Source[] Sources => _sources.ToArray();

		public Instance[] Instances => _instances.Values.ToArray();

	    public Metric[] Metrics => _metrics.ToArray();

		public Instance GetInstance(int aInstanceId)
		{
			return _instances.ContainsKey(aInstanceId)
				? _instances[aInstanceId]
				: _unknownInstance;

			// TODO: if unknown instance then update from api?
		}
	} //end of class
}