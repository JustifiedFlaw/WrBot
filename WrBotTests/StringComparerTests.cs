using Xunit;

namespace WrBotTests
{
    public class StringComparerTests
    {
        [Theory]
        [InlineData("beat", "Beat the game glitchless", "All emeralds glitchless")]
        [InlineData("beat glitchless", "Beat the game glitchless", "All emeralds glitchless")]
        [InlineData("beat glitchless", "Beat the game glitchless", "All emeralds glitched")]
        [InlineData("beat the game glitchless", "Beat the game glitchless", "All emeralds glitchless")]
        [InlineData("game glitchless", "Beat the game glitchless", "All emeralds glitchless")]
        [InlineData("beat the game - glitchless", "Beat the game glitchless", "All emeralds glitchless")]
        [InlineData("emerals", "All emeralds glitchless", "Beat the game glitchless")]
        [InlineData("Any%", "Any%", "All red berries")]
        [InlineData("Any %", "Any%", "All red berries")]
        [InlineData("Any", "Any%", "All red berries")]
        [InlineData("berries", "All red berries", "Any%")]
        [InlineData("Any% Normal", "Any% Normal", "Any% Hard")]
        [InlineData("Any% Nrmak", "Any% Normal", "Any% Hard")]
        // [InlineData("playing dread, lets go", "Metroid Dread", "Paladins")] TODO
        [InlineData("I'm playing metroid dread tonight", "Metroid Dread", "Tetris'D")]
        public void When_Matches_One_More_Then_The_Other_Then_Match_Is_Greater(string input, string expected, string other)
        {
            var matchExpected = StringComparer.PercentWordMatch(expected, input);
            var matchOther = StringComparer.PercentWordMatch(other, input);

            Assert.True(matchExpected > matchOther);
        }
    }
}
