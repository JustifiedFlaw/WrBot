using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Timers;
using SrcRestEase;
using SrcRestEase.Models;
using TwitchRestEase;
using TwitchRestEase.Models;
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

        this.TwitchApi = TwitchRestEase.TwitchApi.Connect(settings.ClientId, settings.AccessToken);

        this.SrcApi = SrcRestEase.SrcApi.Connect();

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
        if (e.Data.Contains(" NOTICE "))
        {
            Log.Warning(e.Data);
        }
        else if (e.Data.Contains(" PRIVMSG "))
        {
            Log.Verbose(e.Data);
        }
        else if(!e.Data.Contains(" PART ")
            && !e.Data.Contains(" JOIN ")
            && !e.Data.Contains(" USERSTATE "))
        {
            Log.Debug(e.Data);
        }
    }

    private void TwitchClient_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        try
        {
            this.ChatCommandAnalyzer.Analyze(e.ChatMessage.Message);

            if (this.ChatCommandAnalyzer.Command != ChatCommands.None)
            {
                Log.Information($"#{e.ChatMessage.Channel} {e.ChatMessage.Username} says: {e.ChatMessage.Message}");
            }

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
        catch (Exception ex)
        {
            Log.Error(ex.Message + "\n" + ex.StackTrace);
        } 
    }

    private void SendMessage(string channel, string message)
    {
        var truncated = Truncate(message, 500);
        Log.Information($"Replied: #{channel} {truncated}");
        this.TwitchClient.SendMessage(channel, truncated);
    }

    private string Truncate(string message, int maxLength)
    {
        if (message.Length > maxLength)
        {
            return message.Substring(0, maxLength - 3) + "...";   
        }
        
        return message;
    }

    public void JoinChannel(string channel)
    {
        this.TwitchClient.JoinChannel(channel);

        if(this.OnJoinedChannel != null)
        {
            this.OnJoinedChannel.Invoke(this, new OnBotJoinedChannelArgs
            {
                Channel = channel
            });
        }
        
        SendMessage(this.Settings.BotName, $"Joined {channel}");
    }

    public void LeaveChannel(string channel)
    {
        this.TwitchClient.LeaveChannel(channel);

        if(this.OnLeftChannel != null)
        {
            this.OnLeftChannel.Invoke(this, new OnBotLeftChannelArgs
            {
                Channel = channel
            });
        }

        SendMessage(this.Settings.BotName, $"Left {channel}");
    }

    private void GetWr(ChannelSettings channelSettings)
    {
        var streamInfo = GetStreamInfo(channelSettings.Name);
        
        var game = DetermineGame(channelSettings.Game, streamInfo.GameName, streamInfo.Title);
        if (game == null)
        {
            SendMessage(channelSettings.Name, "A game could not be determined from the Twitch category. Please use \"!wr [game]\" or \"!wr -setgame [game]\"");
            return;
        }

        var category = DetermineCategory(channelSettings.Category, game, streamInfo.Title);

        var runs = GetRuns(game, category);

        if (runs.Length == 0)
        {
            SendMessage(channelSettings.Name, $"There are no registered runs for {game.Names.International} {category.FullName}");   
        }
        else if(runs.Length > 0)
        {
            var runnerNames = string.Join(", ", runs.SelectMany(r => r.Run.Players.Select(p => p.Name ?? GetRunner(p.Id).Names.International)));

            SendMessage(channelSettings.Name, $"World record for {game.Names.International} {category.FullName} is {runs[0].Run.Times.PrimaryTimeSpan.Format()} by {runnerNames}");
        }
    }

    private Placement[] GetRuns(Game game, CategoryVariable category)
    {
        if (category.HasVariable)
        {
            var subCategoryDictionary = category.SubCategories.ToDictionary(s => "var-" + s.Id, s => s.ValueId);

            return this.SrcApi.GetLeaderboard(game.Id, category.CategoryId, subCategoryDictionary).Result.Data.Runs;
        }
        else
        {
            return this.SrcApi.GetLeaderboard(game.Id, category.CategoryId).Result.Data.Runs;            
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
            SendMessage(channelSettings.Name, "A game could not be determined from the Twitch category or stream title. Please use \"!pb [runner] [game] [category]\" or \"!pb -setgame [game]\"");
            return;
        }

        var category = DetermineCategory(channelSettings.Category, game, streamInfo.Title);

        var personalBests = this.SrcApi.GetPersonalBests(runner.Id, game.Id).Result.Data
            .Where(pb => MatchRunCategory(pb, category));

        if (personalBests.Count() == 0)
        {
            SendMessage(channelSettings.Name, $"No personal bests were found for {runner.Names.International} in {game.Names.International} {category.FullName}");
            return;
        }
        var pb = personalBests.First();

        SendMessage(channelSettings.Name, $"{runner.Names.International}'s pb for {game.Names.International} {category.FullName} is {pb.Run.Times.PrimaryTimeSpan.Format()} (#{pb.Place})");
    }

    private bool MatchRunCategory(PersonalBest pb, CategoryVariable category)
    {
        if (!pb.Run.CategoryId.Equals(category.CategoryId, StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }

        foreach (var subCategory in category.SubCategories)
        {
            if(!pb.Run.Values.ContainsKey(subCategory.Id) 
                || pb.Run.Values[subCategory.Id] != subCategory.ValueId)
            {
                return false;
            }
        }

        return true;
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
            SendMessage(channelName, $"The runner {runnerName} could not be found on speedrun .com. Please use \"!pb [runner]\" or \"!pb -setrunner [runner]");
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
            // uncomment only if the game pick is decent enough
            // var similarites = GamesList.Data?.Select(g => 
            //     new KeyValuePair<Game, decimal>(g, 
            //         StringComparer.PercentWordMatch(g.Names.International, streamTitle)))
            //     .Where(kvp => kvp.Value > 50m)
            //     .OrderByDescending(kvp => kvp.Value);
            
            // if (similarites == null || similarites.Count() == 0)
            // {
            //     return null;
            // }

            // return similarites.First().Key;

            return null;
        }
        else
        {
            return GetGameByName(gameName);
        }
    }

    private CategoryVariable DetermineCategory(DefaultValueSettings categoryDefault, Game game, string streamTitle)
    {
        string categorySearch = this.ChatCommandAnalyzer.HasCategory || this.ChatCommandAnalyzer.HasSetCategory 
            ? this.ChatCommandAnalyzer.Category
            : (
                categoryDefault.Enabled 
                    ? categoryDefault.Value
                    : streamTitle
            );

        var category = MemoryCache.Default[$"Category {game.Id} {categorySearch}"] as CategoryVariable;
        if(category == null)
        {
            var categories = GetCategoriesWithVariables(game.Id);

            var similarities = categories.Select(
                c => new KeyValuePair<CategoryVariable, decimal>(c, 
                StringComparer.PercentWordMatch(c.FullName, categorySearch)))
                .OrderByDescending(kvp => kvp.Value);

            category = similarities.First().Key;

            MemoryCache.Default.Set($"Category {game.Id} {categorySearch}", category, DateTimeOffset.Now.AddHours(1));
        }

        return category;
    }

    private List<CategoryVariable> GetCategoriesWithVariables(string gameId)
    {
        var categories = this.SrcApi.GetGameCategories(gameId).Result.Data
                .Where(c => c.Type == "per-game");

        var result = new List<CategoryVariable>(categories.Count());

        foreach (var category in categories)
        {
            var subCategories = this.SrcApi.GetCategoryVariables(category.Id).Result.Data
                .Where(v => v.IsSubCategory)
                .ToList();

            var flattened = GetSubCategoryCombos(category, subCategories);

            result.AddRange(flattened);
        }

        return result;
    }

    public static IEnumerable<CategoryVariable> GetSubCategoryCombos(Category category, IEnumerable<Variable> remainingSubCategories)
    {
        if (remainingSubCategories.Count() == 0)
        {
            return new List<CategoryVariable>
            {
                new CategoryVariable
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    SubCategories = new List<SubCategory>()
                }
            };
        }
        else if (remainingSubCategories.Count() == 1)
        {
            var remaining = remainingSubCategories.First();

            return remaining.Values.Values.Select(v => new CategoryVariable
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                SubCategories = new List<SubCategory>
                {
                    new SubCategory
                    {
                        Id = remaining.Id,
                        ValueId = v.Key,
                        ValueName = v.Value.Label
                    }
                }
            });
        }
        else
        {
            var current = remainingSubCategories.First();

            var outputs = new List<CategoryVariable>();
            var combos = GetSubCategoryCombos(category, remainingSubCategories.Skip(1));

            foreach (var currentValue in current.Values.Values)
            {
                foreach (var combo in combos)
                {
                    var subCategory = new CategoryVariable
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        SubCategories = new List<SubCategory>
                        {
                            new SubCategory
                            {
                                Id = current.Id,
                                ValueId = currentValue.Key,
                                ValueName = currentValue.Value.Label
                            }
                        }
                    };

                    subCategory.SubCategories.AddRange(combo.SubCategories);

                    outputs.Add(subCategory);
                }
            }

            return outputs;
        }
    }

    private Game GetGameByName(string gameName)
    {
        var game = MemoryCache.Default["Game " + gameName] as Game;
        if (game == null)
        {
            game = this.SrcApi.GetGameByName(gameName).Result.Data.FirstOrDefault();

            if(game != null)
            {
                MemoryCache.Default.Set("Game " + gameName, game, DateTimeOffset.Now.AddHours(1));
            }
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