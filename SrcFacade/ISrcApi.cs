using System.Collections.Generic;
using System.Threading.Tasks;
using RestEase;
using SrcFacade.Models;

namespace SrcFacade
{
    [BasePath("v1")]
    [Header("User-Agent", "RestEase")]
    public interface ISrcApi
    {
        [Get("leaderboards/{gameId}/category/{categoryId}?top=1")]
        Task<GetLeaderboardResponse> GetLeaderboard([Path] string gameId, [Path] string categoryId);

        [Get("leaderboards/{gameId}/category/{categoryId}?var-{variableId}={valueId}&top=1")]
        Task<GetLeaderboardResponse> GetLeaderboard([Path] string gameId, [Path] string categoryId, [Path] string variableId, [Path] string valueId);

        [Get("leaderboards/{gameId}/category/{categoryId}?&top=1")]
        Task<GetLeaderboardResponse> GetLeaderboard([Path] string gameId, [Path] string categoryId, [QueryMap] IDictionary<string, string> variableValues);

        [Get("users/{userId}")]
        Task<GetUserResponse> GetUser([Path] string userId);

        [Get("games?name={name}&_bulk=yes&max=1&orderby=similarity")]
        Task<GetGameByNameResponse> GetGameByName([Path] string name);

        [Get("games/{gameId}/categories?orderby=pos")]
        Task<GetGameCategoriesResponse> GetGameCategories([Path] string gameId);

        [Get("users/{userId}/personal-bests?game={gameId}")]
        Task<GetPersonalBestsResponse> GetPersonalBests([Path] string userId, [Path] string gameId);

        [Get("games?_bulk=yes&max={max}&offset={offset}")]
        Task<GetGamesResponse> GetGames([Path] int max, [Path] int offset);

        [Get("categories/{categoryId}/variables")]
        Task<GetCategoryVariablesResponse> GetCategoryVariables([Path] string categoryId);
    }
}