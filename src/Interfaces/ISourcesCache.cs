using MonikTerminal.ModelsApp;
using System.Threading.Tasks;

namespace MonikTerminal.Interfaces
{
	public interface ISourcesCache
	{
		Group[] Groups { get; }
		Source[] Sources { get; }
		Instance[] Instances { get; }

		Instance GetInstance(int aInstanceId);

		Task Reload();
	}
}