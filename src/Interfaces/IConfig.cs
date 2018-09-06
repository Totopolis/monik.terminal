using MonikTerminal.Enums;

namespace MonikTerminal.Interfaces
{
    public interface IConfig
    {
        void Load(string aFileName);

        string             ServerUrl                { get; set; }
        string             TimeTemplate             { get; set; }
        string             DoubledTimeTemplate      { get; set; }
        int                MaxSourceLen             { get; set; }
        int                MaxInstanceLen           { get; set; }
        int                RefreshPeriod            { get; set; }
        int                KeepAliveWarnSeconds     { get; set; }
        LevelType          LevelFilter              { get; set; }
        SeverityCutoffType SeverityCutoff           { get; set; }
        bool               ShowLevelVerbose         { get; set; }
        int                Top                      { get; set; }
        TerminalMode       Mode                     { get; set; }
    }
}