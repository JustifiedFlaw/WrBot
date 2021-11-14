using System;
using SrcRestEase.Models;
using TwitchRestEase.Models;
using WrBotTests.Builders;
using WrBotTests.Mocks;
using Xunit;

namespace WrBotTests
{
    public class BotTests
    {
        private Bot Bot;
        BotSettings Settings;
        TwitchApiMock TwitchApiMock;
        SrcApiMock SrcApiMock;
        TwitchClientMock TwitchClientMock;
        ChatCommandAnalyzer ChatCommandAnalyzer;

        public BotTests()
        {
            this.Settings = new BotSettings
            {
                BotName = "Bot",
                ClientId = "client",
                AccessToken = "",
                KeepAlive = 3000,
                Channels = new ChannelSettings[]
                {
                    new ChannelSettings
                    {
                        Name = "channel",
                        Runner = new DefaultValueSettings(),
                        Game = new DefaultValueSettings(),
                        Category = new DefaultValueSettings()
                    }
                }
            };
            this.TwitchApiMock = new TwitchApiMock();
            this.SrcApiMock = new SrcApiMock();
            this.TwitchClientMock = new TwitchClientMock();
            this.ChatCommandAnalyzer = new ChatCommandAnalyzer();

            this.Bot = new Bot(
                this.Settings,
                this.TwitchApiMock.Object,
                this.SrcApiMock.Object,
                this.TwitchClientMock.Object,
                this.ChatCommandAnalyzer
            );
        }

        [Fact]
        public void When_Asking_For_Wr_Then_Wr_For_Default_Game_Is_Returned()
        {
            const string channel = "channel";

            var game = GameBuilder.Init().Build();
            var category = CategoryBuilder.Init().Build();
            var streamInfo = StreamBuilder.Init()
                .WithGameName(game.Names.International)
                .Build();
            var run = RunBuilder.Init().Build();

            this.TwitchApiMock.WhenStream(channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);

            this.TwitchClientMock.RaiseMessageReceived(channel, "!wr");

            var expectedMessage = $"World record for {game.Names.International} {category.Name} is {run.Times.PrimaryTimeSpan.Format()} by {run.Players[0].Name}";
            this.TwitchClientMock.ThenSendMessageCalled(channel, expectedMessage);
        }

        [Fact]
        public void When_Runner_Name_Is_Not_Returned_Then_It_Is_Queried_From_SpeedrunDotCom()
        {
        }

        [Fact]
        public void When_Stream_Is_Offline_Then_Message_That_Game_Cannot_Be_Determined()
        {
            const string channel = "channel";

            this.TwitchApiMock.WhenStream(channel, null);

            this.TwitchClientMock.RaiseMessageReceived(channel, "!wr");

            var expectedMessage = "A game could not be determined from the Twitch category. Please use \"!wr [game]\" or \"!wr -setgame [game]\"";
            this.TwitchClientMock.ThenSendMessageCalled(channel, expectedMessage);
        }

        [Fact]
        public void When_Asking_To_Join_Then_Channel_Is_Added()
        {
        }

        [Fact]
        public void When_Asking_To_Join_From_Another_Channel_Than_Bot_Then_Command_Is_Ignored()
        {
        }

        [Fact]
        public void When_Asking_To_Leave_Then_Channel_Is_Removed()
        {
        }

        [Fact]
        public void When_Asking_To_Leave_From_Another_Channel_Than_Bot_Then_Command_Is_Ignored()
        {
        }

        [Fact]
        public void When_Asking_To_Reset_Then_Default_Values_Are_Reset()
        {
        }

        [Fact]
        public void When_Asking_To_Reset_And_Not_Moderator_Or_Broadcaster_Then_Command_Is_Ignored()
        {
        }

        [Fact]
        public void When_Specifying_Game_Then_Game_From_Command_Is_Used()
        {
        }

        [Fact]
        public void When_Specifying_Category_Then_Category_From_Command_Is_Used()
        {
        }

        [Fact]
        public void When_Setting_Game_Then_Game_Is_Changed()
        {
        }

        [Fact]
        public void When_Setting_Category_Then_Category_Is_Changed()
        {
        }

        [Fact]
        public void When_No_Runs_For_Game_And_Category_Then_Message_Is_Sent()
        {
        }

        [Fact]
        public void When_Multiple_Runners_Then_They_Are_Concactenated()
        {
        }

        [Fact]
        public void When_More_Than_5_Runners_Then_They_Are_Truncated()
        {
        }

        [Fact]
        public void When_More_Than_500_Characters_Then_Message_Is_Truncated()
        {
        }

        [Fact]
        public void When_Asking_For_Pb_Then_It_Is_Returned()
        {
        }

        [Fact]
        public void When_Specifying_Runner_Then_Runner_From_Command_Is_Used()
        {
        }

        [Fact]
        public void When_Specifying_Game_For_Pb_Then_Game_From_Command_Is_Used()
        {
        }

        [Fact]
        public void When_Specifying_Category_For_Pb_Then_Category_From_Command_Is_Used()
        {
        }

        [Fact]
        public void When_Game_Has_Sub_Categories_Then_They_Are_Used()
        {
        }
    }
}