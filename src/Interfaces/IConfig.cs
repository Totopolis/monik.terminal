namespace MonikTerminal.Interfaces
{
	public interface IConfig
	{
		string ServerUrl { get; set; }
		string TimeTemplate { get; set; }
		int MaxSourceLen { get; set; }
		int MaxInstanceLen { get; set; }
		int RefreshPeriod { get; set; }
	}
}