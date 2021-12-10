using System;
using System.IO;
using System.Linq;
using FluentMigrator.Runner;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.DependencyInjection;
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
            var appSettings = AppSettingsFactory.Load();

            DatabaseMigration(appSettings.DatabaseSettings);

            var nhSessionFactory = CreateSessionFactory(appSettings.DatabaseSettings);
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

        private static ISessionFactory CreateSessionFactory(DatabaseSettings databaseSettings)
        {
            var nhConfiguration = Fluently.Configure();
            nhConfiguration.Database(PostgreSQLConfiguration.Standard
                .ConnectionString(databaseSettings.ConnectionString));

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

        private static void DatabaseMigration(DatabaseSettings databaseSettings)
        {
            using (var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddPostgres()
                    .WithGlobalConnectionString(databaseSettings.ConnectionString)
                    .ScanIn(typeof(AddChannelsAndLogsTables).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .BuildServiceProvider(false))
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
                    runner.MigrateUp();
                }
            }
        }
    }
}
