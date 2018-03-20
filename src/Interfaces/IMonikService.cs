using MonikTerminal.ModelsApi;
using System.Threading.Tasks;

namespace MonikTerminal.Interfaces
{
    public interface IMonikService
    {
        Task<ESource[]>             GetSources();
        Task<EInstance[]>           GetInstances();
        Task<EGroup[]>              GetGroups();
        Task<ELog_[]>               GetLogs(ELogRequest             aRequest);
        Task<EKeepAlive_[]>         GetKeepAlives(EKeepAliveRequest aRequest);
        Task<EMetric_[]>            GetCurrentMetrics();
        Task<EMetricDescription_[]> GetMetricDescriptions();
        Task<EMetric_[]>            GetHistoryMetrics(EMetricHistoryRequest metricHistoryRequest);
    }
}