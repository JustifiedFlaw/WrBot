using System.Collections.Generic;
using System.Linq;
using Xunit;

using SrcFacade.Models;
using WrBotTests.Builders;
using WrBotTests.Mocks;

namespace WrBotTests
{
    public class BotTests
    {
        const string Channel = "channel";
     
        private Bot Bot;
        BotSettings Settings;
        TwitchApiMock TwitchApiMock;
        SrcApiMock SrcApiMock;
        TwitchClientMock TwitchClientMock;

        public BotTests()
        {
            this.Settings = BotSettingsBuilder.Init()
                .WithChannel(Channel)
                .Build();
            this.TwitchApiMock = new TwitchApiMock();
            this.SrcApiMock = new SrcApiMock();
            this.TwitchClientMock = new TwitchClientMock();

            this.Bot = new Bot(
                this.Settings,
                this.TwitchApiMock.Object,
                this.SrcApiMock.Object,
                this.TwitchClientMock.Object
            );
        }

        [Fact]
        public void When_Asking_For_Wr_Then_Wr_For_Default_Game_Is_Returned()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "wr");

            var expectedMessage = $"World record for {game.Names.International} {category.Name} is {run.Times.PrimaryTimeSpan.Format()} by {run.Players[0].Name}";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_Runner_Name_Is_Not_Returned_Then_It_Is_Queried_From_SpeedrunDotCom()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init()
                .WithPlayerName(null)
                .Build();
            var runnerName = RandomStringBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);
            this.SrcApiMock.WhenRunner(run.Players[0].Id, runnerName);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "wr");

            var expectedMessage = $"World record for {game.Names.International} {category.Name} is {run.Times.PrimaryTimeSpan.Format()} by {runnerName}";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_Stream_Is_Offline_Then_Message_That_Game_Cannot_Be_Determined()
        {
            this.TwitchApiMock.WhenStream(Channel, null);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "wr");

            var expectedMessage = "A game could not be determined from the Twitch category. Please use \"!wr [game]\" or \"!wr -setgame [game]\"";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_Asking_To_Join_Then_Channel_Is_Added()
        {
            var message = ChatMessageBuilder.Init()
                .WithChannel(this.Settings.BotName)
                .WithMessage("joinme")
                .Build();

            this.TwitchClientMock.RaiseChatCommandReceived(this.Settings.BotName, "joinme", message);

            this.TwitchClientMock.ThenJoinedChannel(message.Username);
        }

        [Fact]
        public void When_Asking_To_Join_From_Another_Channel_Than_Bot_Then_Command_Is_Ignored()
        {
            var message = ChatMessageBuilder.Init()
                .WithChannel(Channel)
                .Build();

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "joinme", message);

            this.TwitchClientMock.ThenNotJoinedChannel(message.Username);
        }

        [Fact]
        public void When_Asking_To_Leave_Then_Channel_Is_Removed()
        {
            var message = ChatMessageBuilder.Init()
                .WithChannel(this.Settings.BotName)
                .Build();

            this.TwitchClientMock.RaiseChatCommandReceived(this.Settings.BotName, "leaveme", message);

            this.TwitchClientMock.ThenLeaveChannel(message.Username);
        }

        [Fact]
        public void When_Asking_To_Leave_From_Another_Channel_Than_Bot_Then_Command_Is_Ignored()
        {
            var message = ChatMessageBuilder.Init()
                .WithChannel(Channel)
                .Build();

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "leaveme", message);

            this.TwitchClientMock.ThenNotLeaveChannel(message.Username);
        }

        [Fact]
        public void When_Moderator_Asking_To_Reset_Then_Default_Values_Are_Reset()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);

            this.Bot.Settings.Channels[0].Game.Set(true, game.Names.International);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, true, false, "wr", "-reset");

            Assert.False(this.Bot.Settings.Channels[0].Game.Enabled);
            Assert.Null(this.Bot.Settings.Channels[0].Game.Value);
        }

        [Fact]
        public void When_Broadcaster_Asking_To_Reset_Then_Default_Values_Are_Reset()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);

            this.Bot.Settings.Channels[0].Game.Set(true, game.Names.International);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, false, true, "wr", "-reset");

            Assert.False(this.Bot.Settings.Channels[0].Game.Enabled);
            Assert.Null(this.Bot.Settings.Channels[0].Game.Value);
        }

        [Fact]
        public void When_Asking_To_Reset_And_Not_Moderator_Or_Broadcaster_Then_Command_Is_Ignored()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);

            this.Bot.Settings.Channels[0].Game.Set(true, game.Names.International);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, false, false, "wr", "-reset");

            Assert.True(this.Bot.Settings.Channels[0].Game.Enabled);
            Assert.NotNull(this.Bot.Settings.Channels[0].Game.Value);
        }

        [Fact]
        public void When_Specifying_Game_Then_Game_From_Command_Is_Used()
        {
            var game1 = GameBuilder.Init().Build();
            var category1 = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game1.Names.International)
                .Build();
            var run1 = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game1);
            this.SrcApiMock.WhenCategories(game1.Id, category1);
            this.SrcApiMock.WhenSubCategories(category1.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game1.Id, category1.Id, run1);

            var game2 = GameBuilder.Init().Build();
            var category2 = CategoryBuilder.Init().Build();
            var run2 = RunBuilder.Init().Build();

            this.SrcApiMock.WhenGame(game2);
            this.SrcApiMock.WhenCategories(game2.Id, category2);
            this.SrcApiMock.WhenSubCategories(category2.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game2.Id, category2.Id, run2);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "wr", game2.Names.International);

            var expectedMessage = $"World record for {game2.Names.International} {category2.Name} is {run2.Times.PrimaryTimeSpan.Format()} by {run2.Players[0].Name}";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_Specifying_Category_Then_Category_From_Command_Is_Used()
        {
            var game = GameBuilder.Init().Build();
            var category1 = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run1 = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category1);
            this.SrcApiMock.WhenSubCategories(category1.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category1.Id, run1);

            var category2 = CategoryBuilder.Init().Build();
            var run2 = RunBuilder.Init().Build();

            this.SrcApiMock.WhenCategories(game.Id, category2);
            this.SrcApiMock.WhenSubCategories(category2.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category2.Id, run2);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "wr", game.Names.International, category2.Name);

            var expectedMessage = $"World record for {game.Names.International} {category2.Name} is {run2.Times.PrimaryTimeSpan.Format()} by {run2.Players[0].Name}";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_Setting_Game_Then_Game_Is_Changed()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, false, true, "wr", "-setgame", game.Names.International);

            Assert.True(this.Bot.Settings.Channels[0].Game.Enabled);
            Assert.Equal(game.Names.International, this.Bot.Settings.Channels[0].Game.Value);
        }

        [Fact]
        public void When_Setting_Game_And_Not_Moderator_Or_Broadcaster_Then_Command_Is_Ignored()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, false, false, "wr", "-setgame", game.Names.International);

            Assert.False(this.Bot.Settings.Channels[0].Game.Enabled);
            Assert.Null(this.Bot.Settings.Channels[0].Game.Value);
        }

        [Fact]
        public void When_Setting_Category_Then_Category_Is_Changed()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, false, true, "wr", "-setcategory", category.Name);

            Assert.True(this.Bot.Settings.Channels[0].Category.Enabled);
            Assert.Equal(category.Name, this.Bot.Settings.Channels[0].Category.Value);
        }

        [Fact]
        public void When_Setting_Category_And_Not_Moderator_Or_Broadcaster_Then_Command_Is_Ignored()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, false, false, "wr", "-setcategory", category.Name);

            Assert.False(this.Bot.Settings.Channels[0].Category.Enabled);
            Assert.Null(this.Bot.Settings.Channels[0].Category.Value);
        }
        
        [Fact]
        public void When_Setting_Runner_Then_Runner_Is_Changed()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);

            var runnerName = RandomStringBuilder.Init().Build();

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, false, true, "pb", "-setrunner", runnerName);

            Assert.True(this.Bot.Settings.Channels[0].Runner.Enabled);
            Assert.Equal(runnerName, this.Bot.Settings.Channels[0].Runner.Value);
        }

        [Fact]
        public void When_Setting_Runner_And_Not_Moderator_Or_Broadcaster_Then_Command_Is_Ignored()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);
            
            var runnerName = RandomStringBuilder.Init().Build();

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "wr", "-setrunner", runnerName);

            Assert.False(this.Bot.Settings.Channels[0].Runner.Enabled);
            Assert.Null(this.Bot.Settings.Channels[0].Runner.Value);
        }

        [Fact]
        public void When_No_Runs_For_Game_And_Category_Then_Message_Is_Sent()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "wr");

            var expectedMessage = $"There are no registered runs for {game.Names.International} {category.Name}";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_Multiple_Runners_Then_They_Are_Concactenated()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run1 = RunBuilder.Init().Build();
            var run2 = RunBuilder.Init()
                .WithPrimarySeconds(run1.Times.PrimarySeconds)
                .Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run1, run2);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "wr");

            var expectedMessage = $"World record for {game.Names.International} {category.Name} is {run1.Times.PrimaryTimeSpan.Format()} by {run1.Players[0].Name}, {run2.Players[0].Name}";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_More_Than_6_Runners_Then_They_Are_Truncated()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            
            var runs = new List<Run>(7) 
            {
                RunBuilder.Init().Build()
            };
            for (int i = 0; i < 6; i++)
            {
                runs.Add(RunBuilder.Init()
                    .WithPrimarySeconds(runs[0].Times.PrimarySeconds)
                    .Build());
            }

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, runs.ToArray());

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "wr");

            var expectedNames = string.Join(", ", runs.SelectMany(r => r.Players).Take(5).Select(r => r.Name))
                + " and 2 others";

            var expectedMessage = $"World record for {game.Names.International} {category.Name} is {runs[0].Times.PrimaryTimeSpan.Format()} by {expectedNames}";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_More_Than_500_Characters_Then_Message_Is_Truncated()
        {
            var game = GameBuilder.Init()
                .WithName(RandomStringBuilder.Init().WithLength(500).Build())
                .Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "wr");

            var expectedMessage = "World record for "
                + game.Names.International.Substring(0, 480)
                + "...";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_Asking_For_Pb_Then_It_Is_Returned()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var runner = UserBuilder.Init().WithName(Channel).Build();
            var pb = PersonalBestBuilder.Init()
                .WithCategoryId(category.Id)
                .Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenRunnerByName(runner.Id, runner.Names.International);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenPersonalBests(runner, game.Id, pb);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "pb");

            var expectedMessage = $"{Channel}'s pb for {game.Names.International} {category.Name} is {pb.Run.Times.PrimaryTimeSpan.Format()} (#{pb.Place})";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_Asking_For_Pb_And_No_Runs_Then_Message_Is_Returned()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var runner = UserBuilder.Init().WithName(Channel).Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenRunnerByName(runner.Id, runner.Names.International);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenPersonalBests(runner, game.Id);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "pb");

            var expectedMessage = $"No personal bests were found for {runner.Names.International} in {game.Names.International} {category.Name}";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_Specifying_Runner_Then_Runner_From_Command_Is_Used()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var runner1 = UserBuilder.Init().WithName(Channel).Build();
            var pb1 = PersonalBestBuilder.Init()
                .WithCategoryId(category.Id)
                .Build();
            var runner2 = UserBuilder.Init().Build();
            var pb2 = PersonalBestBuilder.Init()
                .WithCategoryId(category.Id)
                .Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenRunnerByName(runner1.Id, runner1.Names.International);
            this.SrcApiMock.WhenRunnerByName(runner2.Id, runner2.Names.International);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenPersonalBests(runner1, game.Id, pb1);
            this.SrcApiMock.WhenPersonalBests(runner2, game.Id, pb2);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "pb",  runner2.Names.International);

            var expectedMessage = $"{runner2.Names.International}'s pb for {game.Names.International} {category.Name} is {pb2.Run.Times.PrimaryTimeSpan.Format()} (#{pb2.Place})";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_Specifying_Game_For_Pb_Then_Game_From_Command_Is_Used()
        {
            var game1 = GameBuilder.Init().Build();
            var category1 = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game1.Names.International)
                .Build();
            var runner = UserBuilder.Init().WithName(Channel).Build();
            var pb1 = PersonalBestBuilder.Init()
                .WithCategoryId(category1.Id)
                .Build();

            var game2 = GameBuilder.Init().Build();
            var category2 = CategoryBuilder.Init().Build();
            var pb2 = PersonalBestBuilder.Init()
                .WithCategoryId(category2.Id)
                .Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenRunnerByName(runner.Id, runner.Names.International);
            this.SrcApiMock.WhenGame(game1);
            this.SrcApiMock.WhenCategories(game1.Id, category1);
            this.SrcApiMock.WhenSubCategories(category1.Id, new Variable[0]);
            this.SrcApiMock.WhenPersonalBests(runner, game1.Id, pb1);

            this.SrcApiMock.WhenGame(game2);
            this.SrcApiMock.WhenCategories(game2.Id, category2);
            this.SrcApiMock.WhenSubCategories(category2.Id, new Variable[0]);
            this.SrcApiMock.WhenPersonalBests(runner, game2.Id, pb2);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "pb", runner.Names.International, game2.Names.International);

            var expectedMessage = $"{Channel}'s pb for {game2.Names.International} {category2.Name} is {pb2.Run.Times.PrimaryTimeSpan.Format()} (#{pb2.Place})";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_Specifying_Category_For_Pb_Then_Category_From_Command_Is_Used()
        {
            var game = GameBuilder.Init().Build();
            var category1 = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var runner = UserBuilder.Init().WithName(Channel).Build();
            var pb1 = PersonalBestBuilder.Init()
                .WithCategoryId(category1.Id)
                .Build();

            var category2 = CategoryBuilder.Init().Build();
            var pb2 = PersonalBestBuilder.Init()
                .WithCategoryId(category2.Id)
                .Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenRunnerByName(runner.Id, runner.Names.International);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category1, category2);
            this.SrcApiMock.WhenSubCategories(category1.Id, new Variable[0]);
            this.SrcApiMock.WhenSubCategories(category2.Id, new Variable[0]);
            this.SrcApiMock.WhenPersonalBests(runner, game.Id, pb1, pb2);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "pb", runner.Names.International, game.Names.International, category2.Name);

            var expectedMessage = $"{Channel}'s pb for {game.Names.International} {category2.Name} is {pb2.Run.Times.PrimaryTimeSpan.Format()} (#{pb2.Place})";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }

        [Fact]
        public void When_Game_Has_Sub_Categories_Then_They_Are_Used()
        {
            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var variable = VariableBuilder.Init().WithValueCount(2).Build();
            var variableValue2 = variable.Values.Values.Last();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(Channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, variable);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, variable.Id, variableValue2.Key, run);

            this.TwitchClientMock.RaiseChatCommandReceived(Channel, "wr", game.Names.International, category.Name + " " + variableValue2.Value.Label);

            var expectedMessage = $"World record for {game.Names.International} {category.Name} {variableValue2.Value.Label} is {run.Times.PrimaryTimeSpan.Format()} by {run.Players[0].Name}";
            this.TwitchClientMock.ThenSendMessageCalled(Channel, expectedMessage);
        }
    }
}