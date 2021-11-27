using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace WrBot
{
    public class CommandLineInterpreter
    {
        private AppSettings AppSettings;
        private Bot Bot;

        public CommandLineInterpreter(AppSettings appSettings, Bot bot)
        {
            this.AppSettings = appSettings;
            this.Bot = bot;

            Bot.OnJoinedChannel += Bot_OnJoinedChannel;
            Bot.OnLeftChannel += Bot_OnLeftChannel;

            foreach (var channelSettings in AppSettings.BotSettings.Channels)
            {
                channelSettings.Runner.OnSetDefaultChanged += DefaultValue_Changed;
                channelSettings.Game.OnSetDefaultChanged += DefaultValue_Changed;
                channelSettings.Category.OnSetDefaultChanged += DefaultValue_Changed;
            }
        }

        private void DefaultValue_Changed(object sender, OnSetDefaultChangedArgs e)
        {
            SaveSettings();
        }

        private void Bot_OnLeftChannel(object sender, OnBotLeftChannelArgs e)
        {
            if(AppSettings.BotSettings.Channels.Count(c => c.Name.EqualsIgnoreCase(e.Channel)) > 0)
            {
                AppSettings.BotSettings.Channels = 
                    AppSettings.BotSettings.Channels.Where(c => !c.Name.EqualsIgnoreCase(e.Channel))
                    .ToArray();

                SaveSettings();
            }
        }

        private void Bot_OnJoinedChannel(object sender, OnBotJoinedChannelArgs e)
        {
            if(AppSettings.BotSettings.Channels.Count(c => c.Name.EqualsIgnoreCase(e.Channel)) == 0)
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

        private void SaveSettings()
        {
            var jsonString = JsonConvert.SerializeObject(AppSettings, Formatting.Indented);
            File.WriteAllText(
                Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "appsettings.json",
                jsonString);
        }

        public void Read()
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
                        if (consoleAnalyzer.Channel.EqualsIgnoreCase(AppSettings.BotSettings.BotName))
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