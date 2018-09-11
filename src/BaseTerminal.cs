using MonikTerminal.Interfaces;
using System;
using System.Threading.Tasks;

namespace MonikTerminal
{
    public abstract class BaseTerminal : ITerminal
    {
        private int _windowWidth;

        protected readonly IMonikService Service;
        protected readonly IConfig Config;
        protected readonly ISourcesCache SourceCache;

        protected BaseTerminal(IMonikService aService, IConfig aConfig, ISourcesCache aSourceCache)
        {
            Service = aService;
            Config = aConfig;
            SourceCache = aSourceCache;
        }

        protected void SoftClear()
        {
            var newWindowWidth = Console.WindowWidth;
            if (_windowWidth != newWindowWidth)
            {
                _windowWidth = newWindowWidth;
                Console.Clear();
            }
            else
                Console.SetCursorPosition(0, 0);
        }

        protected void WriteWithColor(string val, ConsoleColor? color = null, ConsoleColor? fontColor = null)
        {
            if (color.HasValue)
                Console.BackgroundColor = color.Value;
            if (fontColor.HasValue)
                Console.ForegroundColor = fontColor.Value;

            Console.Write(val);

            Console.ResetColor();
        }

        protected abstract void OnStart();
        protected abstract void Show();

        protected CommonConfig ConfigCommon => Config.Common;

        public void Start()
        {
            OnStart();

            while (true)
            {
                try
                {
                    Show();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"{DateTime.Now.ToString(ConfigCommon.TimeTemplate)} INTERNAL ERROR: {ex.Message}");
                }

                if (ConfigCommon.Mode == TerminalMode.Single)
                    return;

                Task.Delay(ConfigCommon.RefreshPeriod * 1000).Wait();
            }
        }
    } //end of class
}