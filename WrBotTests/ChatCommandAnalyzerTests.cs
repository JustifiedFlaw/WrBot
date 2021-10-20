using Xunit;

namespace WrBotTests
{
    public class ChatCommandAnalyzerTests
    {
        [Fact]
        public void When_Chat_Message_Is_JoinMe_Then_Command_Is_JoinMe()
        {
            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!joinme");

            Assert.Equal(ChatCommands.JoinMe, chatCommandAnalyzer.Command);
        }
        
        [Fact]
        public void When_Chat_Message_Is_LeaveMe_Then_Command_Is_LeaveMe()
        {
            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!leaveme");

            Assert.Equal(ChatCommands.LeaveMe, chatCommandAnalyzer.Command);
        }

        [Fact]
        public void When_Chat_Message_Is_Wr_Then_Command_Is_Wr()
        {
            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!wr");

            Assert.Equal(ChatCommands.Wr, chatCommandAnalyzer.Command);
        }

        [Fact]
        public void When_Chat_Message_Starts_With_Wr_Then_Command_Is_Wr()
        {
            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!wr Game");

            Assert.Equal(ChatCommands.Wr, chatCommandAnalyzer.Command);
        }
        
        [Fact]
        public void When_Chat_Message_Starts_With_Wr_And_Non_Whitespace_Then_Command_Is_None()
        {
            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!wrbla");

            Assert.Equal(ChatCommands.None, chatCommandAnalyzer.Command);
        }

        [Fact]
        public void When_Chat_Message_Is_Pb_Then_Command_Is_Pb()
        {
            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!pb");

            Assert.Equal(ChatCommands.Pb, chatCommandAnalyzer.Command);
        }

        [Fact]
        public void When_Chat_Message_Starts_With_Pb_Then_Command_Is_Pb()
        {
            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!pb runner");

            Assert.Equal(ChatCommands.Pb, chatCommandAnalyzer.Command);
        }
        
        [Fact]
        public void When_Chat_Message_Starts_With_Pb_And_Non_Whitespace_Then_Command_Is_None()
        {
            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!pbbla");

            Assert.Equal(ChatCommands.None, chatCommandAnalyzer.Command);
        }

        [Fact]
        public void When_Wr_Request_Contains_Game_Then_Game_Is_Filled()
        {
            const string gameName = "Game";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!wr " + gameName);

            Assert.True(chatCommandAnalyzer.HasGame);
            Assert.Equal(chatCommandAnalyzer.Game, gameName);
        }

        [Fact]
        public void When_Wr_Request_Contains_Game_Name_In_Quotes_Then_Game_Is_Filled()
        {
            const string gameName = "Game name";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!wr \"" + gameName + "\"");

            Assert.True(chatCommandAnalyzer.HasGame);
            Assert.Equal(chatCommandAnalyzer.Game, gameName);
        }

        [Fact]
        public void When_Wr_Request_Contains_Category_Then_Category_Is_Filled()
        {
            const string categoryName = "Any%";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!wr game " + categoryName);

            Assert.True(chatCommandAnalyzer.HasCategory);
            Assert.Equal(chatCommandAnalyzer.Category, categoryName);
        }

        [Fact]
        public void When_Wr_Request_Contains_Category_Name_In_Quotes_Then_Category_Is_Filled()
        {
            const string categoryName = "No wrong warp";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!wr game \"" + categoryName + "\"");

            Assert.True(chatCommandAnalyzer.HasCategory);
            Assert.Equal(chatCommandAnalyzer.Category, categoryName);
        }

        [Fact]
        public void When_Pb_Request_Contains_Runner_Then_Runner_Is_Filled()
        {
            const string runnerName = "runner";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!pb " + runnerName);

            Assert.True(chatCommandAnalyzer.HasRunner);
            Assert.Equal(chatCommandAnalyzer.Runner, runnerName);
        }

        [Fact]
        public void When_Pb_Request_Contains_Runner_Name_In_Quotes_Then_Runner_Is_Filled()
        {
            const string runnerName = "runner name";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!pb \"" + runnerName + "\"");

            Assert.True(chatCommandAnalyzer.HasRunner);
            Assert.Equal(chatCommandAnalyzer.Runner, runnerName);
        }

        [Fact]
        public void When_Pb_Request_Contains_Game_Then_Game_Is_Filled()
        {
            const string gameName = "Game";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!pb runner " + gameName);

            Assert.True(chatCommandAnalyzer.HasGame);
            Assert.Equal(chatCommandAnalyzer.Game, gameName);
        }

        [Fact]
        public void When_Pb_Request_Contains_Game_Name_In_Quotes_Then_Game_Is_Filled()
        {
            const string gameName = "Game name";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!pb runner \"" + gameName + "\"");

            Assert.True(chatCommandAnalyzer.HasGame);
            Assert.Equal(chatCommandAnalyzer.Game, gameName);
        }

        [Fact]
        public void When_Pb_Request_Contains_Category_Then_Category_Is_Filled()
        {
            const string categoryName = "Any%";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!pb runner game " + categoryName);

            Assert.True(chatCommandAnalyzer.HasCategory);
            Assert.Equal(chatCommandAnalyzer.Category, categoryName);
        }

        [Fact]
        public void When_Pb_Request_Contains_Category_Name_In_Quotes_Then_Category_Is_Filled()
        {
            const string categoryName = "No wrong warp";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!pb runner game \"" + categoryName + "\"");

            Assert.True(chatCommandAnalyzer.HasCategory);
            Assert.Equal(chatCommandAnalyzer.Category, categoryName);
        }

        [Fact]
        public void When_Using_SetRunner_Then_HasSetRunner_Is_True()
        {
            const string runnerName = "runner";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!wr -setrunner " + runnerName);

            Assert.True(chatCommandAnalyzer.HasSetRunner);
            Assert.Equal(chatCommandAnalyzer.Runner, runnerName);

            Assert.False(chatCommandAnalyzer.HasSetGame);
            Assert.False(chatCommandAnalyzer.HasSetCategory);
        }

        [Fact]
        public void When_Using_SetGame_Then_HasSetGame_Is_True()
        {
            const string gameName = "game";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!wr -setgame " + gameName);

            Assert.True(chatCommandAnalyzer.HasSetGame);
            Assert.Equal(chatCommandAnalyzer.Game, gameName);

            Assert.False(chatCommandAnalyzer.HasSetRunner);
            Assert.False(chatCommandAnalyzer.HasSetCategory);
        }

        [Fact]
        public void When_Using_SetCategory_Then_HasSetCategory_Is_True()
        {
            const string categoryName = "category";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!wr -setcategory " + categoryName);

            Assert.True(chatCommandAnalyzer.HasSetCategory);
            Assert.Equal(chatCommandAnalyzer.Category, categoryName);

            Assert.False(chatCommandAnalyzer.HasSetRunner);
            Assert.False(chatCommandAnalyzer.HasSetGame);
        }

        [Fact]
        public void When_Using_Sets_With_Quotes_Then_Values_Are_Filled()
        {
            const string runnerName = "runner name";
            const string gameName = "game name";
            const string categoryName = "category name";

            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze($"!pb -setrunner \"{runnerName}\" -setgame \"{gameName}\" -setcategory \"{categoryName}\"");

            Assert.True(chatCommandAnalyzer.HasSetRunner);
            Assert.True(chatCommandAnalyzer.HasSetGame);
            Assert.True(chatCommandAnalyzer.HasSetCategory);

            Assert.Equal(chatCommandAnalyzer.Runner, runnerName);
            Assert.Equal(chatCommandAnalyzer.Game, gameName);
            Assert.Equal(chatCommandAnalyzer.Category, categoryName);
        }

        [Fact]
        public void When_Chat_Message_Contains_Reset_Then_HasReset_Set_To_True()
        {
            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!wr -reset");

            Assert.True(chatCommandAnalyzer.HasReset);
        }

        [Fact]
        public void When_Chat_Message_Does_Not_Contain_Reset_Then_HasReset_Set_To_False()
        {
            var chatCommandAnalyzer = new ChatCommandAnalyzer();

            chatCommandAnalyzer.Analyze("!wr -setgame game");

            Assert.False(chatCommandAnalyzer.HasReset);
        }
    }
}
