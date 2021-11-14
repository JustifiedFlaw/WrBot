using System.Linq;
using System.Collections.Generic;
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

        public void WhenWorldRecordRuns(string gameId, string categoryId, params Run[] runs)
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

        public void WhenWorldRecordRuns(string gameId, string categoryId, string variableId, string valueId, params Run[] runs)
        {
            this.Setup(x => x.GetLeaderboard(gameId, categoryId, new Dictionary<string, string> { { "var-" + variableId, valueId } }))
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

        public void WhenRunnerByName(string id, string runnerName)
        {
            this.Setup(x => x.GetUser(runnerName))
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

        public void WhenPersonalBests(User runner, string gameId, params PersonalBest[] pbs)
        {
            this.Setup(x => x.GetPersonalBests(runner.Id, gameId))
                .ReturnsAsync(new GetPersonalBestsResponse
                {
                    Data = pbs
                });
        }
    }
}