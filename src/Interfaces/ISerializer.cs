namespace MonikTerminal.Interfaces
{
    public interface ISerializer
    {
        void LoadConfig(string path);
        void WriteNewMetricsToConfig(string path);
    }
}