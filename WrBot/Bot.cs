using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Timers;
using RestEase;
using RestEase.Interfaces;
using RestEase.Models.Src;
using RestEase.Models.TwitchApi;
using Serilog;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

public class Bot
{
    public BotSettings Settings { get; private set; }

    public ITwitchApi TwitchApi { get; private set; }
    public ISrcApi SrcApi { get; private set; }

    public TwitchClient TwitchClient { get; private set; }

    public ChatCommandAnalyzer ChatCommandAnalyzer { get; private set; }

    public Timer KeepTwitchConnectionAlive { get; set; }

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

        this.KeepTwitchConnectionAlive = new Timer(this.Settings.KeepAlive);
        this.KeepTwitchConnectionAlive.Elapsed += KeepTwitchConnectionAlive_Elapsed;
        this.KeepTwitchConnectionAlive.Start();
    }

    private TwitchClient InitializeTwitchClient()
    {
        var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
        var customClient = new WebSocketClient(clientOptions);
        var client = new TwitchClient(customClient);

        var credentials = new ConnectionCredentials(this.Settings.BotName, this.Settings.AccessToken);

        client.Initialize(credentials, this.Settings.Channels.Select(c => c.Name).ToList());

        client.OnMessageReceived += TwitchClient_OnMessageReceived;
        client.OnLog += TwitchClient_OnLog;
        client.OnDisconnected += TwitchClient_OnDisconnected;

        client.Connect();

        foreach (var channelSettings in this.Settings.Channels)
        {
            client.JoinChannel(channelSettings.Name);
        }

        return client;
    }

    private void KeepTwitchConnectionAlive_Elapsed(object sender, ElapsedEventArgs e)
    {
        this.TwitchClient.SendRaw("PING");
    }

    private void TwitchClient_OnDisconnected(object sender, OnDisconnectedEventArgs e)
    {
        if (!this.TwitchClient.IsConnected)
        {
            Log.Information("Twitch client disconnected. Attempting to reconnect...");

            while (!this.TwitchClient.IsConnected)
            {
                this.TwitchClient.Connect();
                System.Threading.Thread.Sleep(5000);
            }

            foreach (var channelSettings in this.Settings.Channels)
            {
                this.TwitchClient.JoinChannel(channelSettings.Name);
            }
        }
    }

    private void TwitchClient_OnLog(object sender, OnLogArgs e)
    {
        if (e.Data.Contains("NOTICE"))
        {
            Log.Warning(e.Data);
        }
        else
        {
            Log.Debug(e.Data);
        }
    }

    private void TwitchClient_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        try
        {
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

                //TODO: allow admin to join a specific channel
            }
            
            var channelSettings = this.Settings.Channels.First(c => c.Name.Equals(e.ChatMessage.Channel, StringComparison.InvariantCultureIgnoreCase));
            if(e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
            {
                if (this.ChatCommandAnalyzer.HasReset)
                {
                    Log.Information($"Resetting defaults for channel {e.ChatMessage.Channel}");

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
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        } 
    }

    public void JoinChannel(string channel)
    {
        Log.Information($"Joining channel {channel}");

        this.TwitchClient.JoinChannel(channel);

        if(this.OnJoinedChannel != null)
        {
            this.OnJoinedChannel.Invoke(this, new OnBotJoinedChannelArgs
            {
                Channel = channel
            });
        }
        
        TwitchClient.SendMessage(this.Settings.BotName, $"Joined {channel}");
    }

    public void LeaveChannel(string channel)
    {
        Log.Information($"Leaving channel {channel}");

        this.TwitchClient.LeaveChannel(channel);

        if(this.OnLeftChannel != null)
        {
            this.OnLeftChannel.Invoke(this, new OnBotLeftChannelArgs
            {
                Channel = channel
            });
        }

        TwitchClient.SendMessage(this.Settings.BotName, $"Left {channel}");
    }

    private void GetWr(ChannelSettings channelSettings)
    {
        var streamInfo = GetStreamInfo(channelSettings.Name);
        
        var game = DetermineGame(channelSettings.Game, streamInfo.GameName, streamInfo.Title);
        if (game == null)
        {
            this.TwitchClient.SendMessage(channelSettings.Name, "A game could not be determined from the Twitch category. Please use \"!wr [game]\" or \"!wr -setgame [game]\"");
            return;
        }

        var category = DetermineCategory(channelSettings.Category, game, streamInfo.Title);

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
        var streamInfo = GetStreamInfo(channelSettings.Name);
        
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

    private Stream GetStreamInfo(string channel)
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
                MemoryCache.Default.Set("Stream " + channel, streamInfo, DateTimeOffset.Now.AddMinutes(15));
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

        var category = MemoryCache.Default[$"Category {game.Id} {categorySearch}"] as Category;
        if(category == null)
        {
            var categories = this.SrcApi.GetGameCategories(game.Id).Result.Data
                .Where(c => c.Type == "per-game");

            var similarities = categories.Select(
                c => new KeyValuePair<Category, decimal>(c, 
                StringComparer.PercentWordMatch(c.Name, categorySearch)))
                .OrderByDescending(kvp => kvp.Value);

            category = similarities.First().Key;

            MemoryCache.Default.Set($"Category {game.Id} {categorySearch}", category, DateTimeOffset.Now.AddHours(1));
        }

        return category;
    }

    private Game GetGameByName(string gameName)
    {
        var game = MemoryCache.Default["Game " + gameName] as Game;
        if (game == null)
        {
            game = this.SrcApi.GetGameByName(gameName).Result.Data.FirstOrDefault();

            MemoryCache.Default.Set("Game " + gameName, game, DateTimeOffset.Now.AddHours(1));
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
                MemoryCache.Default.Set("Runner " + idOrName, runner, DateTimeOffset.Now.AddHours(1));
            }
            catch
            {
                runner = null;
            }
        }

        return runner;
    }
}