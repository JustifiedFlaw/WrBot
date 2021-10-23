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

            var bot = new Bot(AppSettings.BotSettings);
            bot.OnJoinedChannel += Bot_OnJoinedChannel;
            bot.OnLeftChannel += Bot_OnLeftChannel;

            foreach (var channelSettings in AppSettings.BotSettings.Channels)
            {
                channelSettings.Runner.OnSetDefaultChanged += DefaultValue_Changed;
                channelSettings.Game.OnSetDefaultChanged += DefaultValue_Changed;
                channelSettings.Category.OnSetDefaultChanged += DefaultValue_Changed;
            }

            Log.Information("Listening to " + string.Join(", ", AppSettings.BotSettings.Channels.Select(c => c.Name)));
            Console.WriteLine("Type 'quit' to close the WrBot");
            
            while (Console.ReadLine() != "quit");
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
    }
}
