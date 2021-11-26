using Xunit;

namespace WrBotTests
{
    public class CommandEventArgsTests
    {
        [Fact]
        public void When_Wr_Request_Contains_Game_Then_Game_Is_Filled()
        {
            const string gameName = "Game name";

            var commandEventArgs = new CommandEventArgs("wr", new [] { gameName });

            Assert.True(commandEventArgs.HasGame);
            Assert.Equal(commandEventArgs.Game, gameName);
        }

        [Fact]
        public void When_Wr_Request_Contains_Category_Then_Category_Is_Filled()
        {
            const string categoryName = "No wwrong warp";

            var commandEventArgs = new CommandEventArgs("wr", new [] { "game", categoryName });

            Assert.True(commandEventArgs.HasCategory);
            Assert.Equal(commandEventArgs.Category, categoryName);
        }

        [Fact]
        public void When_Pb_Request_Contains_Runner_Then_Runner_Is_Filled()
        {
            const string runnerName = "runner";

            var commandEventArgs = new CommandEventArgs("pb", new [] { runnerName });
            
            Assert.True(commandEventArgs.HasRunner);
            Assert.Equal(commandEventArgs.Runner, runnerName);
        }

        [Fact]
        public void When_Pb_Request_Contains_Game_Then_Game_Is_Filled()
        {
            const string gameName = "Game";

            var commandEventArgs = new CommandEventArgs("pb", new [] { "Runner", gameName });

            Assert.True(commandEventArgs.HasGame);
            Assert.Equal(commandEventArgs.Game, gameName);
        }

        [Fact]
        public void When_Pb_Request_Contains_Category_Then_Category_Is_Filled()
        {
            const string categoryName = "Any%";

            var commandEventArgs = new CommandEventArgs("pb", new [] { "runner", "game", categoryName });

            Assert.True(commandEventArgs.HasCategory);
            Assert.Equal(commandEventArgs.Category, categoryName);
        }

        [Fact]
        public void When_Using_SetRunner_Then_HasSetRunner_Is_True()
        {
            const string runnerName = "runner";

            var commandEventArgs = new CommandEventArgs("wr", new [] { "-setrunner", runnerName });

            Assert.True(commandEventArgs.HasSetRunner);
            Assert.Equal(commandEventArgs.Runner, runnerName);

            Assert.False(commandEventArgs.HasSetGame);
            Assert.False(commandEventArgs.HasSetCategory);
        }

        [Fact]
        public void When_Using_SetGame_Then_HasSetGame_Is_True()
        {
            const string gameName = "game";

            var commandEventArgs = new CommandEventArgs("wr", new [] { "-setgame", gameName });

            Assert.True(commandEventArgs.HasSetGame);
            Assert.Equal(commandEventArgs.Game, gameName);

            Assert.False(commandEventArgs.HasSetRunner);
            Assert.False(commandEventArgs.HasSetCategory);
        }

        [Fact]
        public void When_Using_SetCategory_Then_HasSetCategory_Is_True()
        {
            const string categoryName = "category";

            var commandEventArgs = new CommandEventArgs("wr", new [] { "-setcategory", categoryName });

            Assert.True(commandEventArgs.HasSetCategory);
            Assert.Equal(commandEventArgs.Category, categoryName);

            Assert.False(commandEventArgs.HasSetRunner);
            Assert.False(commandEventArgs.HasSetGame);
        }

        [Fact]
        public void When_Chat_Message_Contains_Reset_Then_HasReset_Set_To_True()
        {
            var commandEventArgs = new CommandEventArgs("wr", new [] { "-reset" });

            Assert.True(commandEventArgs.HasReset);
        }

        [Fact]
        public void When_Chat_Message_Does_Not_Contain_Reset_Then_HasReset_Set_To_False()
        {
            var commandEventArgs = new CommandEventArgs("wr", new [] { "game" });

            Assert.False(commandEventArgs.HasReset);
        }
    }
}
