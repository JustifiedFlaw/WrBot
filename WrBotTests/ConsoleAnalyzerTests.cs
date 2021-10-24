using Xunit;

namespace WrBotTests
{
    public class ConsoleAnalyzerTests
    {
        [Theory]
        [InlineData("", ConsoleCommands.None)]
        [InlineData("alsjdh", ConsoleCommands.None)]
        [InlineData("joinme", ConsoleCommands.None)]
        [InlineData("leaveme", ConsoleCommands.None)]
        [InlineData("quitme", ConsoleCommands.None)]
        [InlineData("join", ConsoleCommands.Join)]
        [InlineData("join channel", ConsoleCommands.Join)]
        [InlineData("Join channel", ConsoleCommands.Join)]
        [InlineData("leave", ConsoleCommands.Leave)]
        [InlineData("leave channel", ConsoleCommands.Leave)]
        [InlineData("Leave channel", ConsoleCommands.Leave)]
        [InlineData("quit", ConsoleCommands.Quit)]
        [InlineData("Quit", ConsoleCommands.Quit)]
        public void When_Console_Line_Is_Command_Then_Correct_Command_Is_Selected(string line, ConsoleCommands expected)
        {
            var consoleAnalyzer = new ConsoleAnalyzer();

            consoleAnalyzer.Analyze(line);

            Assert.Equal(expected, consoleAnalyzer.Command);
        }

        [Fact]
        public void When_Joining_Then_Channel_Is_Filled()
        {
            const string channel = "channel";

            var consoleAnalyzer = new ConsoleAnalyzer();

            consoleAnalyzer.Analyze("join " + channel);

            Assert.Equal(ConsoleCommands.Join, consoleAnalyzer.Command);
            Assert.True(consoleAnalyzer.HasChannel);
            Assert.Equal(channel, consoleAnalyzer.Channel);
        }

        [Fact]
        public void When_Leaving_Then_Channel_Is_Filled()
        {
            const string channel = "channel";

            var consoleAnalyzer = new ConsoleAnalyzer();

            consoleAnalyzer.Analyze("leave " + channel);

            Assert.Equal(ConsoleCommands.Leave, consoleAnalyzer.Command);
            Assert.True(consoleAnalyzer.HasChannel);
            Assert.Equal(channel, consoleAnalyzer.Channel);
        }

        [Fact]
        public void When_Not_Joining_Or_Leaving_Then_Channel_Is_Empty()
        {
            var consoleAnalyzer = new ConsoleAnalyzer();

            consoleAnalyzer.Analyze("ailsdhih");

            Assert.Equal(ConsoleCommands.None, consoleAnalyzer.Command);
            Assert.False(consoleAnalyzer.HasChannel);
            Assert.Null(consoleAnalyzer.Channel);
        }
    }
}