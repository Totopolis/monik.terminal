using MonikTerminal.Enums;

namespace MonikTerminal.Interfaces
{
    public interface IConfig
    {
        void Load(string aFileName);

        CommonConfig Common { get; }
        LogConfig Log { get; }
        KeepAliveConfig KeepAlive { get; }
        MetricsConfig Metrics { get; }
    }
}