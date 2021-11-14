using System;
using System.Linq;
using Moq;
using SrcRestEase;
using SrcRestEase.Models;

namespace WrBotTests.Mocks
{
    public class SrcApiMock : Mock<ISrcApi>
    {
        public void WhenGame(Game game)
        {
            this.Setup(x => x.GetGameByName(game.Names.International))
                .ReturnsAsync(new GetGameByNameResponse
                {
                    Data = new Game[] 
                    {
                        game
                    }
                });
        }

        public void WhenCategories(string gameId, params Category[] categories)
        {
            this.Setup(x => x.GetGameCategories(gameId))
                .ReturnsAsync(new GetGameCategoriesResponse
                {
                    Data = categories
                });
        }

        public void WhenSubCategories(string cateogryId, params Variable[] variables)
        {
            this.Setup(x => x.GetCategoryVariables(cateogryId))
                .ReturnsAsync(new GetCategoryVariablesResponse
                {
                    Data = variables
                });
        }

        internal void WhenWorldRecordRuns(string gameId, string categoryId, params Run[] runs)
        {
            this.Setup(x => x.GetLeaderboard(gameId, categoryId))
                .ReturnsAsync(new GetLeaderboardResponse
                {
                    Data = new Leaderboard
                    {
                        Runs = runs.Select(r => new Placement
                        {
                            Place = 1,
                            Run = r
                        }).ToArray()
                    }
                });
        }

        public void WhenRunner(string id, string runnerName)
        {
            this.Setup(x => x.GetUser(id))
                .ReturnsAsync(new GetUserResponse
                {
                    Data = new User
                    {
                        Id = id,
                        Names = new UserNames
                        {
                            International = runnerName
                        }
                    }
                });
        }
    }
}