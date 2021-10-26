using System;
using System.Collections.Generic;
using System.Linq;
using SrcRestEase.Models;
using Xunit;

namespace WrBotTests
{
    public class GamesListPickTests
    {
        public GamesListPickTests()
        {
            var gamesListRefresher = new GamesListRefresher(new GamesListRefresherSettings
            {
                DelayInMilliseconds = 8000000
            });

            gamesListRefresher.LoadSavedData();
        }

        [Theory]
        [InlineData("Sonic 1", "Sonic 1 (2013)")]
        public void When_Given_A_Title_Then_Pick_Correct_Game(string streamTitle, string expectedGameName)
        {
            var similarites = GamesList.Data?.Select(g => 
                new KeyValuePair<Game, decimal>(g, 
                    StringComparer.PercentWordMatch(g.Names.International, streamTitle)))
                .Where(kvp => kvp.Value > 50m)
                .OrderByDescending(kvp => kvp.Value);
            
            Assert.NotNull(similarites);
            Assert.True(similarites.Count() > 0);

            var game = similarites.First().Key;
            Assert.Equal(expectedGameName, game.Names.International, System.StringComparer.InvariantCultureIgnoreCase);
        }
    }
}