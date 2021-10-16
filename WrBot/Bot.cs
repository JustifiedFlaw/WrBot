using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using RestEase;
using RestEase.Interfaces;
using RestEase.Models.Src;
using RestEase.Models.TwitchApi;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

public class Bot
{
    public BotSettings Settings { get; private set; }

    public ITwitchApi TwitchApi { get; private set; }

    public ISrcApi SrcApi { get; private set; }

    public TwitchClient TwitchClient { get; private set; }

    public ChatCommandAnalyzer ChatCommandAnalyzer { get; private set; }

    public SetDefault SetRunner { get; private set; }

    public SetDefault SetGame { get; private set; }

    public SetDefault SetCategory { get; private set; }

    public Bot(BotSettings settings)
    {
        this.Settings = settings;

        this.TwitchApi = RestClient.For<ITwitchApi>("https://api.twitch.tv");
        this.TwitchApi.ClientId = settings.ClientId;
        this.TwitchApi.Authorization = "Bearer " + settings.AccessToken;

        this.SrcApi = RestClient.For<ISrcApi>("https://www.speedrun.com/api");

        this.TwitchClient = InitializeTwitchClient();

        this.ChatCommandAnalyzer = new ChatCommandAnalyzer();

        this.SetRunner = new SetDefault();
        this.SetGame = new SetDefault();
        this.SetCategory = new SetDefault();
    }

    private TwitchClient InitializeTwitchClient()
    {
        ConnectionCredentials credentials = new ConnectionCredentials(this.Settings.BotName, this.Settings.AccessToken);
	        var clientOptions = new ClientOptions
                {
                    MessagesAllowedInPeriod = 750,
                    ThrottlingPeriod = TimeSpan.FromSeconds(30)
                };
        WebSocketClient customClient = new WebSocketClient(clientOptions);
        var client = new TwitchClient(customClient);

        foreach (var channel in this.Settings.Channels)
        {
            client.Initialize(credentials, channel);
        }

        client.OnMessageReceived += TwitchClient_OnMessageReceived;

        client.Connect();

        foreach (var channel in this.Settings.Channels)
        {
            client.JoinChannel(channel);
        }

        return client;
    }

    private void TwitchClient_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        this.ChatCommandAnalyzer.Analyze(e.ChatMessage.Message);

        this.SetRunner.Set(this.ChatCommandAnalyzer.HasSetRunner, this.ChatCommandAnalyzer.Runner);
        this.SetGame.Set(this.ChatCommandAnalyzer.HasSetGame, this.ChatCommandAnalyzer.Game);
        this.SetCategory.Set(this.ChatCommandAnalyzer.HasSetCategory, this.ChatCommandAnalyzer.Category);

        if (this.ChatCommandAnalyzer.IsWr)
        {
            GetWr(e.ChatMessage.Channel);
        }
        else if(this.ChatCommandAnalyzer.IsPb)
        {
            GetPb(e.ChatMessage.Channel);
        }
    }

    private void GetWr(string channel)
    {
        var streamInfo = GetWrStreamInfo(channel);
        
        var game = DetermineGame(streamInfo.GameName, streamInfo.Title);
        if (game == null)
        {
            this.TwitchClient.SendMessage(channel, "A game could not be determined from the Twitch category or stream title. Please use \"!wr [game]\" or \"!wr -setgame [game]\"");
            return;
        }

        var category = DetermineCategory(game, streamInfo.Title);

        // TODO: This can cause an exception with certain categories
        var runs = this.SrcApi.GetLeaderboard(game.Id, category.Id).Result.Data.Runs;

        if (runs.Length == 0)
        {
            this.TwitchClient.SendMessage(channel, $"There are no registered runs for {game.Names.International} {category.Name}");   
        }
        else if(runs.Length > 0)
        {
            var runnerNames = string.Join(", ", runs.SelectMany(r => r.Run.Players.Select(p => GetRunnerName(p.Id))));

            this.TwitchClient.SendMessage(channel, $"World record for {game.Names.International} {category.Name} is {runs[0].Run.Times.PrimaryTimeSpan.Format()} by {runnerNames}");
        }
    }

    private void GetPb(string channel)
    {
        // TODO
        // var streamInfo = GetWrStreamInfo(channel);
        
        // var runner = DetermineRunner(channel);
        // if (runner == null)
        // {
        //     this.TwitchClient.SendMessage(channel, $"The runner {} could not be found on speedrun .com. Please use \"!pb [runner]\" or \"!pb -setrunner [runner]");
        //     return;
        // }

        // var game = DetermineGame(streamInfo.GameName, streamInfo.Title);
        // if (game == null)
        // {
        //     this.TwitchClient.SendMessage(channel, "A game could not be determined from the Twitch category or stream title. Please use \"!pb [runner] [game] [category]\" or \"!pb -setgame [game]\"");
        //     return;
        // }

        // var category = DetermineCategory(game, streamInfo.Title);

        // var wr = GetSrcPb(runner, game, category);

        // this.TwitchClient.SendMessage(channel, $"{runner.Names.International}'s pb for {game.Names.International} {category.Name} is {wr.Times.PrimaryTimeSpan}");
    }

    private Stream GetWrStreamInfo(string channel)
    {
        var streamInfo = MemoryCache.Default[channel] as Stream;
        if (streamInfo == null)
        {
            streamInfo = this.TwitchApi.GetStreams(channel).Result.Data.FirstOrDefault();
            if (streamInfo == null) // stream is most likely not live
            {
                streamInfo = new Stream
                {
                    GameName = "",
                    Title = ""
                };
            }
            else
            {
                MemoryCache.Default[channel] = streamInfo;
            }
        }

        return streamInfo;
    }

    private Game DetermineGame(string streamGame, string streamTitle)
    {
        string gameName = this.ChatCommandAnalyzer.HasGame || this.ChatCommandAnalyzer.HasSetGame 
            ? this.ChatCommandAnalyzer.Game
            : (
                this.SetGame.Enabled 
                    ? this.SetGame.Value
                    : streamGame
            );
        
        if (gameName.ToLower() == "retro" || string.IsNullOrWhiteSpace(gameName))
        {
            //TODO: look for game name in title
            return null;
        }
        else
        {
            return GetGameByName(gameName);
        }
    }

    private Category DetermineCategory(Game game, string streamTitle)
    {
        string categorySearch = this.ChatCommandAnalyzer.HasCategory || this.ChatCommandAnalyzer.HasSetCategory 
            ? this.ChatCommandAnalyzer.Category
            : (
                this.SetCategory.Enabled 
                    ? this.SetCategory.Value
                    : streamTitle
            );

        var category = MemoryCache.Default["Category " + categorySearch] as Category;
        if(category == null)
        {
            // TODO: order by same order as speedrun.com
            var categories = this.SrcApi.GetGameCategories(game.Id).Result.Data
                .Where(c => c.Type == "per-game");

            var similarities = categories.Select(
                c => new KeyValuePair<Category, decimal>(c, 
                StringComparer.PercentWordMatch(c.Name, categorySearch)))
                .OrderByDescending(kvp => kvp.Value);

            category = similarities.First().Key;

            MemoryCache.Default["Category " + categorySearch] = category;
        }

        return category;
    }

    private Game GetGameByName(string gameName)
    {
        var game = MemoryCache.Default["Game " + gameName] as Game;
        if (game == null)
        {
            game = this.SrcApi.GetGameByName(gameName).Result.Data.First();

            MemoryCache.Default["Game " + gameName] = game;
        }

        return game;
    }

    private string GetRunnerName(string id)
    {
        var runnerName = MemoryCache.Default["User " + id] as string;
        if (runnerName == null)
        {
            runnerName = this.SrcApi.GetUser(id).Result.Data.Names.International;

            MemoryCache.Default["User " + id] = runnerName;
        }

        return runnerName;
    }
}