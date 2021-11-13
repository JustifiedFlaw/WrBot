using System;
using SrcRestEase.Models;
using TwitchRestEase.Models;
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

            var game = new Game
            {
                Id = "kjhdskjhs",
                Names = new GameNames
                {
                    International = "game name"
                }
            };

            var category = new Category
            {
                Id = "qouwyey",
                Name = "category name",
                Type = "per-game"
            };

            var streamInfo = new Stream
            {
                GameName = game.Names.International,
                Title = "Title"
            };

            var run = new Run
            {
                Players = new Player[] 
                {
                    new Player { Id = null, Name = "Fuzzy" } // TODO: test if name is not there
                },
                Times = new Times
                {
                    PrimarySeconds = 61.5m
                }
            };

            this.TwitchApiMock.WhenStream(channel, streamInfo);
            this.SrcApiMock.WhenGame(game);
            this.SrcApiMock.WhenCategories(game.Id, category);
            this.SrcApiMock.WhenSubCategories(category.Id, new Variable[0]);
            this.SrcApiMock.WhenWorldRecordRuns(game.Id, category.Id, run);

            this.TwitchClientMock.InvokeMessageReceived(channel, "!wr");

            var expectedMessage = $"World record for {game.Names.International} {category.Name} is {run.Times.PrimaryTimeSpan.Format()} by {run.Players[0].Name}";
            this.TwitchClientMock.ThenSendMessageCalled(channel, expectedMessage);
        }

        [Fact]
        public void When_Stream_Is_Offline_Then_Message_That_Game_Cannot_Be_Determined()
        {
            const string channel = "channel";

            this.TwitchApiMock.WhenStream(channel, null);

            this.TwitchClientMock.InvokeMessageReceived(channel, "!wr");

            var expectedMessage = "A game could not be determined from the Twitch category. Please use \"!wr [game]\" or \"!wr -setgame [game]\"";
            this.TwitchClientMock.ThenSendMessageCalled(channel, expectedMessage);
        }
    }
}