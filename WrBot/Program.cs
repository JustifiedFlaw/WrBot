using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace WrBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var botSettings = LoadBotSettings();
            var bot = new Bot(botSettings);

            // TODO: listen to WrBot chat for !joinme, !leaveme

            Console.WriteLine("Listening to " + string.Join(", ", botSettings.Channels));
            Console.WriteLine("Type 'quit' to close the WrBot");
            
            while (Console.ReadLine() != "quit");
        }

        private static BotSettings LoadBotSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            return new BotSettings
            {
                BotName = configuration.GetSection("BotSettings:BotName").Value,
                ClientId = configuration.GetSection("BotSettings:ClientId").Value,
                AccessToken = configuration.GetSection("BotSettings:AccessToken").Value,
                Channels = configuration.GetSection("BotSettings:Channels").Value.Split(',')
            };
        }
    }
}
