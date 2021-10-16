using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace WrBot
{
    class Program
    {
        static AppSettings AppSettings;

        static void Main(string[] args)
        {
            LoadAppSettings();

            var bot = new Bot(AppSettings.BotSettings);
            bot.OnJoinedChannel += Bot_OnJoinedChannel;
            bot.OnLeftChannel += Bot_OnLeftChannel;

            Console.WriteLine("Listening to " + string.Join(", ", AppSettings.BotSettings.Channels));
            Console.WriteLine("Type 'quit' to close the WrBot");
            
            while (Console.ReadLine() != "quit");
        }

        private static void Bot_OnLeftChannel(object sender, OnBotLeftChannelArgs e)
        {
            if(AppSettings.BotSettings.Channels.Contains(e.Channel))
            {
                AppSettings.BotSettings.Channels = 
                    AppSettings.BotSettings.Channels.Where(c => c != e.Channel)
                    .ToArray();

                SaveSettings();
            }
        }

        private static void Bot_OnJoinedChannel(object sender, OnBotJoinedChannelArgs e)
        {
            if(!AppSettings.BotSettings.Channels.Contains(e.Channel))
            {
                AppSettings.BotSettings.Channels = 
                    AppSettings.BotSettings.Channels.Append(e.Channel).ToArray();

                SaveSettings();
            }
        }

        private static void LoadAppSettings()
        {
            var jsonString = File.ReadAllText(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "appsettings.json");
            AppSettings = JsonConvert.DeserializeObject<AppSettings>(jsonString);
        }

        private static void SaveSettings()
        {
            var jsonString = JsonConvert.SerializeObject(AppSettings);
            File.WriteAllText(
                Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "appsettings.json",
                jsonString);
        }
    }
}
