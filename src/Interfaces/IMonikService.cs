using MonikTerminal.ModelsApi;
using System.Threading.Tasks;

namespace MonikTerminal.Interfaces
{
	public interface IMonikService
	{
		Task<ESource[]> GetSources();
		Task<EInstance[]> GetInstances();
		Task<EGroup[]> GetGroups();
		Task<ELog_[]> GetLogs(ELogRequest aRequest);
		Task<EKeepAlive_[]> GetKeepAlives(EKeepAliveRequest aRequest);
	}
}