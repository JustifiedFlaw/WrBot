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
        private IChannelService ChannelService;

        public CommandLineInterpreter(AppSettings appSettings, Bot bot, IChannelService channelService)
        {
            this.AppSettings = appSettings;
            this.Bot = bot;
            this.ChannelService = channelService;

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
            var channel = AppSettings.BotSettings.Channels.First(c => c.Name.EqualsIgnoreCase(e.Channel));
            ChannelService.Update(channel);
        }

        private void Bot_OnLeftChannel(object sender, OnBotLeftChannelArgs e)
        {
            if(AppSettings.BotSettings.Channels.Count(c => c.Name.EqualsIgnoreCase(e.Channel)) > 0)
            {
                AppSettings.BotSettings.Channels = 
                    AppSettings.BotSettings.Channels.Where(c => !c.Name.EqualsIgnoreCase(e.Channel))
                    .ToArray();

                ChannelService.Delete(e.Channel);
            }
        }

        private void Bot_OnJoinedChannel(object sender, OnBotJoinedChannelArgs e)
        {
            if(AppSettings.BotSettings.Channels.Count(c => c.Name.EqualsIgnoreCase(e.Channel)) == 0)
            {
                var newChannel = new ChannelSettings
                {
                    Name = e.Channel,
                    Runner = new DefaultValueSettings(e.Channel),
                    Game = new DefaultValueSettings(e.Channel),
                    Category = new DefaultValueSettings(e.Channel)
                };

                AppSettings.BotSettings.Channels = 
                    AppSettings.BotSettings.Channels.Append(newChannel).ToArray();

                ChannelService.Add(newChannel);
            }
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