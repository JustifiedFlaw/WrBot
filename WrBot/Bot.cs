using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using SrcFacade;
using SrcFacade.Models;
using TwitchFacade;
using TwitchFacade.Models;
using TwitchLib.Client.Interfaces;

public class Bot : TwitchBot
{
    public ITwitchApi TwitchApi { get; private set; }
    public ISrcApi SrcApi { get; private set; }

    public EventHandler<OnBotJoinedChannelArgs> OnJoinedChannel;
    public EventHandler<OnBotLeftChannelArgs> OnLeftChannel;

    private MemoryCache Cache = new MemoryCache("Bot");

    public Bot(BotSettings settings,
        ITwitchApi twitchApi,
        ISrcApi srcApi,
        ITwitchClient twitchClient) 
        : base(settings, twitchClient)
    {
        this.TwitchApi = twitchApi;
        this.SrcApi = srcApi;

        this.Commands.Add("joinme", Command_JoinMe);
        this.Commands.Add("leaveme", Command_LeaveMe);
        this.Commands.Add("wr", Command_Wr);
        this.Commands.Add("pb", Command_Pb);
    }

    private void Command_JoinMe(CommandEventArgs e)
    {
        if (e.Channel.EqualsIgnoreCase(this.Settings.BotName))
        {
            JoinChannel(e.Username);
        }
    }

    public void JoinChannel(string channel)
    {
        this.TwitchClient.JoinChannel(channel);

        if (this.OnJoinedChannel != null)
        {
            this.OnJoinedChannel.Invoke(this, new OnBotJoinedChannelArgs
            {
                Channel = channel
            });
        }

        SendMessage(this.Settings.BotName, $"Joined {channel}");
    }

    private void Command_LeaveMe(CommandEventArgs e)
    {
        if (e.Channel.EqualsIgnoreCase(this.Settings.BotName))
        {
            LeaveChannel(e.Username);
        }
    }

    public void LeaveChannel(string channel)
    {
        this.TwitchClient.LeaveChannel(channel);

        if (this.OnLeftChannel != null)
        {
            this.OnLeftChannel.Invoke(this, new OnBotLeftChannelArgs
            {
                Channel = channel
            });
        }

        SendMessage(this.Settings.BotName, $"Left {channel}");
    }

    private void Command_Wr(CommandEventArgs e)
    {
        var channelSettings = GetChannelSettings(e.Channel);
        CheckForSets(e, channelSettings);

        var streamInfo = GetStreamInfo(channelSettings.Name);

        var game = DetermineGame(channelSettings.Game, streamInfo.GameName, streamInfo.Title, e);
        if (game == null)
        {
            SendMessage(channelSettings.Name, "A game could not be determined from the Twitch category. Please use \"!wr [game]\" or \"!wr -setgame [game]\"");
            return;
        }

        var category = DetermineCategory(channelSettings.Category, game, streamInfo.Title, e);

        var runs = GetRuns(game, category);

        if (runs.Length == 0)
        {
            SendMessage(channelSettings.Name, $"There are no registered runs for {game.Names.International} {category.FullName}");
        }
        else if (runs.Length > 0)
        {
            var runnerNames = GetRunnerNames(runs, 5);

            SendMessage(channelSettings.Name, $"World record for {game.Names.International} {category.FullName} is {runs[0].Run.Times.PrimaryTimeSpan.Format()} by {runnerNames}");
        }
    }

    private void Command_Pb(CommandEventArgs e)
    {
        var channelSettings = GetChannelSettings(e.Channel);
        CheckForSets(e, channelSettings);

        var streamInfo = GetStreamInfo(channelSettings.Name);
        
        var runner = DetermineRunner(channelSettings.Runner, e);
        if (runner == null)
        {
            return;
        }

        var game = DetermineGame(channelSettings.Game, streamInfo.GameName, streamInfo.Title, e);
        if (game == null)
        {
            SendMessage(channelSettings.Name, "A game could not be determined from the Twitch category. Please use \"!pb [runner] [game] [category]\" or \"!pb -setgame [game]\"");
            return;
        }

        var category = DetermineCategory(channelSettings.Category, game, streamInfo.Title, e);

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

    private ChannelSettings GetChannelSettings(string channel)
    {
        return this.Settings.Channels.First(c => c.Name.EqualsIgnoreCase(channel));
    }

    private void CheckForSets(CommandEventArgs e, ChannelSettings channelSettings)
    {
        if (e.IsBroadcaster || e.IsModerator)
        {
            if (e.HasReset)
            {
                channelSettings.Runner.Reset();
                channelSettings.Game.Reset();
                channelSettings.Category.Reset();
            }

            channelSettings.Runner.Set(e.HasSetRunner, e.Runner);
            channelSettings.Game.Set(e.HasSetGame, e.Game);
            channelSettings.Category.Set(e.HasSetCategory, e.Category);
        }
    }

    private string GetRunnerNames(Placement[] runs, int max)
    {
        var runners = runs.SelectMany(r => r.Run.Players).ToList();

        string suffix;
        if (runners.Count > max + 1)
        {
            suffix = $" and {runners.Count - max} others";
            runners = runners.Take(max).ToList();
        }
        else
        {
            suffix = "";
        }

        return string.Join(", ", runners.Select(p => p.Name ?? GetRunner(p.Id).Names.International))
            + suffix;
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

    private bool MatchRunCategory(PersonalBest pb, CategoryVariable category)
    {
        if (!pb.Run.CategoryId.EqualsIgnoreCase(category.CategoryId))
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
        var streamInfo = this.Cache["Stream " + channel] as Stream;
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
                this.Cache.Set("Stream " + channel, streamInfo, DateTimeOffset.Now.AddMinutes(15));
            }
        }

        return streamInfo;
    }

    private User DetermineRunner(DefaultValueSettings runnerDefault, CommandEventArgs e)
    {
        string runnerName = e.HasRunner || e.HasSetRunner 
            ? e.Runner
            : (
                runnerDefault.Enabled 
                    ? runnerDefault.Value
                    : e.Channel
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
            SendMessage(e.Channel, $"The runner {runnerName} could not be found on speedrun .com. Please use \"!pb [runner]\" or \"!pb -setrunner [runner]");
        }

        return runner;
    }

    private Game DetermineGame(DefaultValueSettings gameDefault, string streamGame, string streamTitle, CommandEventArgs e)
    {
        string gameName = e.HasGame || e.HasSetGame 
            ? e.Game
            : (
                gameDefault.Enabled 
                    ? gameDefault.Value
                    : streamGame
            );
        
        if (gameName.ToLower() == "retro" || string.IsNullOrWhiteSpace(gameName))
        {
            return null;
        }
        else
        {
            return GetGameByName(gameName);
        }
    }

    private CategoryVariable DetermineCategory(DefaultValueSettings categoryDefault, Game game, string streamTitle, CommandEventArgs e)
    {
        string categorySearch = e.HasCategory || e.HasSetCategory 
            ? e.Category
            : (
                categoryDefault.Enabled 
                    ? categoryDefault.Value
                    : streamTitle
            );

        var category = this.Cache[$"Category {game.Id} {categorySearch}"] as CategoryVariable;
        if (category == null)
        {
            var categories = GetCategoriesWithVariables(game.Id);

            var similarities = categories.Select(
                c => new KeyValuePair<CategoryVariable, decimal>(c, 
                StringComparer.PercentWordMatch(c.FullName, categorySearch)))
                .OrderByDescending(kvp => kvp.Value);

            category = similarities.First().Key;

            this.Cache.Set($"Category {game.Id} {categorySearch}", category, DateTimeOffset.Now.AddHours(1));
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

            var flattened = SubCategoryAdapter.GetSubCategoryCombos(category, subCategories);

            result.AddRange(flattened);
        }

        return result;
    }

    private Game GetGameByName(string gameName)
    {
        var game = this.Cache["Game " + gameName] as Game;
        if (game == null)
        {
            game = this.SrcApi.GetGameByName(gameName).Result.Data.FirstOrDefault();

            if(game != null)
            {
                this.Cache.Set("Game " + gameName, game, DateTimeOffset.Now.AddHours(1));
            }
        }

        return game;
    }

    private User GetRunner(string idOrName)
    {
        var runner = this.Cache["Runner " + idOrName] as User;
        if (runner == null)
        {
            try
            {
                runner = this.SrcApi.GetUser(idOrName).Result.Data; 
                this.Cache.Set("Runner " + idOrName, runner, DateTimeOffset.Now.AddHours(1));
            }
            catch
            {
                runner = null;
            }
        }

        return runner;
    }
}