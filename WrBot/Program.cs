using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using SrcFacade;
using TwitchFacade;

namespace WrBot
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureLogs();
            
            var appSettings = LoadAppSettings();            

            var twitchApi = TwitchApiFactory.Connect(appSettings.BotSettings.ClientId, appSettings.BotSettings.AccessToken);
            var srcApi = SrcApiFactory.Connect();
            var twitchClient = TwitchClientFactory.Connect(appSettings.BotSettings);
            var bot = new Bot(appSettings.BotSettings, twitchApi, srcApi, twitchClient);
            
            var cli = new CommandLineInterpreter(appSettings, bot);

            Log.Information("Listening to " + string.Join(", ", appSettings.BotSettings.Channels.Select(c => c.Name)));
            Console.WriteLine("Type 'quit' to close the WrBot");

            cli.Read();
        }

        private static AppSettings LoadAppSettings()
        {
            var filePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "appsettings.json";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            var jsonString = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<AppSettings>(jsonString);
        }

        private static void ConfigureLogs()
        {
            var twoWeeks = new System.TimeSpan(14, 0, 0, 0);
            Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Verbose()
                            .WriteTo.File($"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}logs{Path.DirectorySeparatorChar}wrbot.log",
                                restrictedToMinimumLevel: LogEventLevel.Information,
                                rollingInterval: RollingInterval.Day,
                                retainedFileTimeLimit: twoWeeks)
                            .WriteTo.Console(LogEventLevel.Debug)
                            .CreateLogger();
        }
    }
}
