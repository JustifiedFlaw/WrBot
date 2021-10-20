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

    public EventHandler<OnBotJoinedChannelArgs> OnJoinedChannel;
    public EventHandler<OnBotLeftChannelArgs> OnLeftChannel;

    public Bot(BotSettings settings)
    {
        this.Settings = settings;

        this.TwitchApi = RestClient.For<ITwitchApi>("https://api.twitch.tv");
        this.TwitchApi.ClientId = settings.ClientId;
        this.TwitchApi.Authorization = "Bearer " + settings.AccessToken;

        this.SrcApi = RestClient.For<ISrcApi>("https://www.speedrun.com/api");

        this.TwitchClient = InitializeTwitchClient();

        this.ChatCommandAnalyzer = new ChatCommandAnalyzer();
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

        client.Initialize(credentials, this.Settings.Channels.Select(c => c.Name).ToList());

        client.OnMessageReceived += TwitchClient_OnMessageReceived;
        // client.OnLog += TwitchClient_OnLog;

        client.Connect();

        foreach (var channelSettings in this.Settings.Channels)
        {
            client.JoinChannel(channelSettings.Name);
        }

        return client;
    }

    // private void TwitchClient_OnLog(object sender, OnLogArgs e)
    // {
    // }

    private void TwitchClient_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        // TODO: log use

        this.ChatCommandAnalyzer.Analyze(e.ChatMessage.Message);

        if (e.ChatMessage.Channel.Equals(this.Settings.BotName, StringComparison.InvariantCultureIgnoreCase))
        {
            if (this.ChatCommandAnalyzer.Command == ChatCommands.JoinMe)
            {
                JoinChannel(e.ChatMessage.Username);
            }
            else if(this.ChatCommandAnalyzer.Command == ChatCommands.LeaveMe)
            {
                LeaveChannel(e.ChatMessage.Username);
            }
        }
        
        var channelSettings = this.Settings.Channels.First(c => c.Name.Equals(e.ChatMessage.Channel, StringComparison.InvariantCultureIgnoreCase));
        if(e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
        {
            if (this.ChatCommandAnalyzer.HasReset)
            {
                channelSettings.Runner.Reset();
                channelSettings.Game.Reset();
                channelSettings.Category.Reset();
            }

            channelSettings.Runner.Set(this.ChatCommandAnalyzer.HasSetRunner, this.ChatCommandAnalyzer.Runner);
            channelSettings.Game.Set(this.ChatCommandAnalyzer.HasSetGame, this.ChatCommandAnalyzer.Game);
            channelSettings.Category.Set(this.ChatCommandAnalyzer.HasSetCategory, this.ChatCommandAnalyzer.Category);
        }

        if (this.ChatCommandAnalyzer.Command == ChatCommands.Wr)
        {
            GetWr(channelSettings);
        }
        else if(this.ChatCommandAnalyzer.Command == ChatCommands.Pb)
        {
            GetPb(channelSettings);
        }   
    }

    private void JoinChannel(string username)
    {
        this.TwitchClient.JoinChannel(username);

        if(this.OnJoinedChannel != null)
        {
            this.OnJoinedChannel.Invoke(this, new OnBotJoinedChannelArgs
            {
                Channel = username
            });
        }
        
        TwitchClient.SendMessage(this.Settings.BotName, $"Joined {username}");
    }

    private void LeaveChannel(string username)
    {
        this.TwitchClient.LeaveChannel(username);

        if(this.OnLeftChannel != null)
        {
            this.OnLeftChannel.Invoke(this, new OnBotLeftChannelArgs
            {
                Channel = username
            });
        }

        TwitchClient.SendMessage(this.Settings.BotName, $"Left {username}");
    }

    private void GetWr(ChannelSettings channelSettings)
    {
        var streamInfo = GetWrStreamInfo(channelSettings.Name);
        
        var game = DetermineGame(channelSettings.Game, streamInfo.GameName, streamInfo.Title);
        if (game == null)
        {
            this.TwitchClient.SendMessage(channelSettings.Name, "A game could not be determined from the Twitch category or stream title. Please use \"!wr [game]\" or \"!wr -setgame [game]\"");
            return;
        }

        var category = DetermineCategory(channelSettings.Category, game, streamInfo.Title);

        // TODO: This can cause an exception with certain categories
        var runs = this.SrcApi.GetLeaderboard(game.Id, category.Id).Result.Data.Runs;

        if (runs.Length == 0)
        {
            this.TwitchClient.SendMessage(channelSettings.Name, $"There are no registered runs for {game.Names.International} {category.Name}");   
        }
        else if(runs.Length > 0)
        {
            var runnerNames = string.Join(", ", runs.SelectMany(r => r.Run.Players.Select(p => GetRunner(p.Id).Names.International)));

            this.TwitchClient.SendMessage(channelSettings.Name, $"World record for {game.Names.International} {category.Name} is {runs[0].Run.Times.PrimaryTimeSpan.Format()} by {runnerNames}");
        }
    }

    private void GetPb(ChannelSettings channelSettings)
    {
        var streamInfo = GetWrStreamInfo(channelSettings.Name);
        
        var runner = DetermineRunner(channelSettings.Runner, channelSettings.Name);
        if (runner == null)
        {
            return;
        }

        var game = DetermineGame(channelSettings.Game, streamInfo.GameName, streamInfo.Title);
        if (game == null)
        {
            this.TwitchClient.SendMessage(channelSettings.Name, "A game could not be determined from the Twitch category or stream title. Please use \"!pb [runner] [game] [category]\" or \"!pb -setgame [game]\"");
            return;
        }

        var category = DetermineCategory(channelSettings.Category, game, streamInfo.Title);

        var personalBests = this.SrcApi.GetPersonalBests(runner.Id, game.Id).Result.Data
            .Where(pb => pb.Run.CategoryId.Equals(category.Id, StringComparison.InvariantCultureIgnoreCase));
        if (personalBests.Count() == 0)
        {
            this.TwitchClient.SendMessage(channelSettings.Name, $"No personal bests were found for {runner.Names.International} in {game.Names.International} {category.Name}");
            return;
        }
        var pb = personalBests.First();

        this.TwitchClient.SendMessage(channelSettings.Name, $"{runner.Names.International}'s pb for {game.Names.International} {category.Name} is {pb.Run.Times.PrimaryTimeSpan.Format()} (#{pb.Place})");
    }

    private Stream GetWrStreamInfo(string channel)
    {
        var streamInfo = MemoryCache.Default["Stream " + channel] as Stream;
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
                MemoryCache.Default["Stream " + channel] = streamInfo;
            }
        }

        return streamInfo;
    }

    private User DetermineRunner(DefaultValueSettings runnerDefault, string channelName)
    {
        string runnerName = this.ChatCommandAnalyzer.HasRunner || this.ChatCommandAnalyzer.HasSetRunner 
            ? this.ChatCommandAnalyzer.Runner
            : (
                runnerDefault.Enabled 
                    ? runnerDefault.Value
                    : channelName
            );
        
        User runner;
        if (string.IsNullOrWhiteSpace(runnerName))
        {
            runner = null;
        }
        else
        {
            runner = GetRunner(runnerName);
        }

        if (runner == null)
        {
            this.TwitchClient.SendMessage(channelName, $"The runner {runnerName} could not be found on speedrun .com. Please use \"!pb [runner]\" or \"!pb -setrunner [runner]");
        }

        return runner;
    }

    private Game DetermineGame(DefaultValueSettings gameDefault, string streamGame, string streamTitle)
    {
        string gameName = this.ChatCommandAnalyzer.HasGame || this.ChatCommandAnalyzer.HasSetGame 
            ? this.ChatCommandAnalyzer.Game
            : (
                gameDefault.Enabled 
                    ? gameDefault.Value
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

    private Category DetermineCategory(DefaultValueSettings categoryDefault, Game game, string streamTitle)
    {
        string categorySearch = this.ChatCommandAnalyzer.HasCategory || this.ChatCommandAnalyzer.HasSetCategory 
            ? this.ChatCommandAnalyzer.Category
            : (
                categoryDefault.Enabled 
                    ? categoryDefault.Value
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
            game = this.SrcApi.GetGameByName(gameName).Result.Data.FirstOrDefault();

            MemoryCache.Default["Game " + gameName] = game;
        }

        return game;
    }

    private User GetRunner(string idOrName)
    {
        var runner = MemoryCache.Default["Runner " + idOrName] as User;
        if (runner == null)
        {
            try
            {
                runner = this.SrcApi.GetUser(idOrName).Result.Data; 
                MemoryCache.Default["Runner " + idOrName] = runner;
            }
            catch
            {
                runner = null;
            }
        }

        return runner;
    }
}