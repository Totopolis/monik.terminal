namespace MonikTerminal.Interfaces
{
    public interface IConfig
    {
        CommonConfig Common { get; }
        LogConfig Log { get; }
        KeepAliveConfig KeepAlive { get; }
        MetricsConfig Metrics { get; }
        MetricsFillConfig MetricsFill { get; }
    }
}