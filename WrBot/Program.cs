using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace WrBot
{
    class Program
    {
        static AppSettings AppSettings;
        static Bot Bot;
        static GamesListRefresher GamesListRefresher;

        static void Main(string[] args)
        {
            LoadAppSettings();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File($"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}logs{Path.DirectorySeparatorChar}wrbot.log",
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    rollingInterval: RollingInterval.Day)
                .WriteTo.Console(LogEventLevel.Debug)
                .CreateLogger();

            var twitchApi = TwitchRestEase.TwitchApi.Connect(AppSettings.BotSettings.ClientId, AppSettings.BotSettings.AccessToken);
            var srcApi = SrcRestEase.SrcApi.Connect();
            var twitchClient = TwitchClientInitializer.InitializeTwitchClient(AppSettings.BotSettings);
            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            Bot = new Bot(AppSettings.BotSettings, twitchApi, srcApi, twitchClient, chatCommandAnalyzer);
            Bot.OnJoinedChannel += Bot_OnJoinedChannel;
            Bot.OnLeftChannel += Bot_OnLeftChannel;

            foreach (var channelSettings in AppSettings.BotSettings.Channels)
            {
                channelSettings.Runner.OnSetDefaultChanged += DefaultValue_Changed;
                channelSettings.Game.OnSetDefaultChanged += DefaultValue_Changed;
                channelSettings.Category.OnSetDefaultChanged += DefaultValue_Changed;
            }

            GamesListRefresher = new GamesListRefresher(AppSettings.GamesListRefresherSettings);
            GamesListRefresher.Start();

            Log.Information("Listening to " + string.Join(", ", AppSettings.BotSettings.Channels.Select(c => c.Name)));
            Console.WriteLine("Type 'quit' to close the WrBot");

            HandleConsoleCommands();
        }

        private static void DefaultValue_Changed(object sender, OnSetDefaultChangedArgs e)
        {
            SaveSettings();
        }

        private static void Bot_OnLeftChannel(object sender, OnBotLeftChannelArgs e)
        {
            if(AppSettings.BotSettings.Channels.Count(c => c.Name.Equals(e.Channel, StringComparison.InvariantCultureIgnoreCase)) > 0)
            {
                AppSettings.BotSettings.Channels = 
                    AppSettings.BotSettings.Channels.Where(c => !c.Name.Equals(e.Channel, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();

                SaveSettings();
            }
        }

        private static void Bot_OnJoinedChannel(object sender, OnBotJoinedChannelArgs e)
        {
            if(AppSettings.BotSettings.Channels.Count(c => c.Name.Equals(e.Channel, StringComparison.InvariantCultureIgnoreCase)) == 0)
            {
                AppSettings.BotSettings.Channels = 
                    AppSettings.BotSettings.Channels.Append(new ChannelSettings{
                        Name = e.Channel,
                        Runner = new DefaultValueSettings(),
                        Game = new DefaultValueSettings(),
                        Category = new DefaultValueSettings()
                    }).ToArray();

                SaveSettings();
            }
        }

        private static void LoadAppSettings()
        {
            var filePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "appsettings.json";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            var jsonString = File.ReadAllText(filePath);
            AppSettings = JsonConvert.DeserializeObject<AppSettings>(jsonString);
        }

        private static void SaveSettings()
        {
            var jsonString = JsonConvert.SerializeObject(AppSettings, Formatting.Indented);
            File.WriteAllText(
                Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "appsettings.json",
                jsonString);
        }

        private static void HandleConsoleCommands()
        {
            var consoleAnalyzer = new ConsoleAnalyzer();

            do
            {
                consoleAnalyzer.Analyze(Console.ReadLine());

                if (consoleAnalyzer.Command == ConsoleCommands.Join)
                {
                    if (consoleAnalyzer.HasChannel)
                    {
                        Bot.JoinChannel(consoleAnalyzer.Channel);
                    }
                    else
                    {
                        Console.WriteLine("Expected syntax: join channel");
                    }
                }

                if (consoleAnalyzer.Command == ConsoleCommands.Leave)
                {
                    if (consoleAnalyzer.HasChannel)
                    {
                        if (consoleAnalyzer.Channel.Equals(AppSettings.BotSettings.BotName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            Console.WriteLine("You cannot leave the bot's own channel");
                        }
                        else
                        {
                            Bot.LeaveChannel(consoleAnalyzer.Channel);                        
                        }
                    }
                    else
                    {
                        Console.WriteLine("Expected syntax: leave channel");
                    }
                }
            } while (consoleAnalyzer.Command != ConsoleCommands.Quit);
        }
    }
}
