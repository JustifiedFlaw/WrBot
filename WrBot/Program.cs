using System;
using System.IO;
using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Newtonsoft.Json;
using NHibernate;
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
            var appSettings = LoadAppSettings();

            var nhSessionFactory = CreateSessionFactory(appSettings.NHSettings);
            var logService = new LogService(nhSessionFactory);
            var channelService = new ChannelService(nhSessionFactory);

            appSettings.BotSettings.Channels = channelService.GetAll();

            ConfigureLogs(logService);
            
            var twitchApi = TwitchApiFactory.Connect(appSettings.BotSettings.ClientId, appSettings.BotSettings.AccessToken);
            var srcApi = SrcApiFactory.Connect();
            var twitchClient = TwitchClientFactory.Connect(appSettings.BotSettings);
            var bot = new Bot(appSettings.BotSettings, twitchApi, srcApi, twitchClient);
            
            var cli = new CommandLineInterpreter(appSettings, bot, channelService);

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

        private static ISessionFactory CreateSessionFactory(NHSettings nhSettings)
        {
            var nhConfiguration = Fluently.Configure();
            nhConfiguration.Database(PostgreSQLConfiguration.Standard
                .ConnectionString(c => {
                    c.Host(nhSettings.Host);
                    c.Port(nhSettings.Port);
                    c.Database(nhSettings.Database);
                    c.Username(nhSettings.User);
                    c.Password(nhSettings.Password);
                }));

            nhConfiguration.Mappings(m => m.FluentMappings.AddFromAssemblyOf<LogItem>());

            return nhConfiguration.BuildSessionFactory();
        }

        private static void ConfigureLogs(ILogService logService)
        {
            var twoWeeks = new System.TimeSpan(14, 0, 0, 0);
            Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Verbose()
                            .WriteTo.Console(LogEventLevel.Debug)
                            .WriteTo.NHibernateSink(logService, LogEventLevel.Information)
                            .CreateLogger();
        }
    }
}
