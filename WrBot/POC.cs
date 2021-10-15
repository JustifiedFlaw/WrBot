/* using System;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using RestEase.Interfaces;
using RestEase;
using RestEase.Models.TwitchApi;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace WrBot
{
    class POC
    {
        static string BotName;
        static string AccessToken;
        static string ChannelName;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            BotName = configuration.GetSection("BotSettings:BotName").Value;
            // ClientId = configuration.GetSection("BotSettings:ClientId").Value;
            AccessToken = configuration.GetSection("BotSettings:AccessToken").Value;
            ChannelName = configuration.GetSection("BotSettings:Channels").Value;

            // GetSrcGame("Super Mario Bros"); //smb1 Id: om1m3625
            
            // GetWrByNames("Super Mario Bros", "Any%");

            // SendChatMessage();

            StartChatListen();
         
            System.Console.ReadLine();
        }

        private static void GetWrByNames(string gameName, string streamTitle)
        {
            var api = RestClient.For<ISrcApi>("https://www.speedrun.com/api");

            var game = api.GetGameByName(gameName).Result.Data.First();

            if (game != null)
            {
                var categories = api.GetGameCategories(game.Id).Result.Data;

                var bestMatch = categories.Where(c => streamTitle.Contains(c.Name)).First();

                if (bestMatch != null)
                {
                    var leaderboard = api.GetLeaderboard(game.Id, bestMatch.Id).Result.Data;

                    var user = api.GetUser(leaderboard.Runs[0].Run.Players[0].Id).Result.Data;
                    System.Console.WriteLine($"{user.Names.International} in {leaderboard.Runs[0].Run.Times.PrimaryTimeSpan}");
                }
            }
        }

        private static void GetSrcGame(string name)
        {
            var api = RestClient.For<ISrcApi>("https://www.speedrun.com/api");

            var game = api.GetGameByName(name).Result.Data.First();

            if (game != null)
            {
                System.Console.WriteLine(game.Id);
            }
        }

        private static void GetWr()
        {
            var api = RestClient.For<ISrcApi>("https://www.speedrun.com/api");

            var leaderboard = api.GetLeaderboard("xldev513", "rklg3rdn").Result.Data;

            var user = api.GetUser(leaderboard.Runs[0].Run.Players[0].Id).Result.Data;
            System.Console.WriteLine($"{user.Names.International} in {leaderboard.Runs[0].Run.Times.PrimaryTimeSpan}");
        }

        private static void GetTwitchStream()
        {
            ITwitchApi api = RestClient.For<ITwitchApi>("https://api.twitch.tv");

            Streams streams = api.GetStreams(ChannelName).Result;
            foreach (var stream in streams.Data)
            {
                Console.WriteLine($"Game: {stream.GameName}. Title: {stream.Title}");                
            }
        }

        private static void StartChatListen()
        {
            ConnectionCredentials credentials = new ConnectionCredentials(BotName, AccessToken);
	        var clientOptions = new ClientOptions
                {
                    MessagesAllowedInPeriod = 750,
                    ThrottlingPeriod = TimeSpan.FromSeconds(30)
                };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            var client = new TwitchClient(customClient);
            client.Initialize(credentials, ChannelName);

            client.OnMessageReceived += Client_OnMessageReceived;

            client.Connect();
        }

        private static void SendChatMessage()
        {
            ConnectionCredentials credentials = new ConnectionCredentials(BotName, AccessToken);
	        var clientOptions = new ClientOptions
                {
                    MessagesAllowedInPeriod = 750,
                    ThrottlingPeriod = TimeSpan.FromSeconds(30)
                };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            var client = new TwitchClient(customClient);
            client.Initialize(credentials, ChannelName);
            client.Connect();
            client.JoinChannel(ChannelName);

            client.SendMessage(ChannelName, "Testing from wr bot");
        }

        private static void Monitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            System.Console.WriteLine(e.Stream.GameName);
        }

        private static void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            System.Console.WriteLine(e.Stream.GameName);
        }

        private static void Monitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e)
        {
            System.Console.WriteLine(e.Stream.GameName);
        }

        private static void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            System.Console.WriteLine(e.ChatMessage.DisplayName + " says " + e.ChatMessage.Message);
        }
    }
}
 */