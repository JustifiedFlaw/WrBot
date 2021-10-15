using System.Threading.Tasks;
using RestEase.Models.Src;
using RestEase.Models.TwitchApi;

namespace RestEase.Interfaces
{
    [BasePath("v1")]
    [Header("User-Agent", "RestEase")]
    public interface ISrcApi
    {
        [Get("leaderboards/{gameId}/category/{categoryId}?top=1")]
        Task<GetLeaderboardResponse> GetLeaderboard([Path] string gameId, [Path] string categoryId);

        [Get("users/{userId}")]
        Task<GetUserResponse> GetUser([Path] string userId);

        [Get("games?name={name}&_bulk=yes&max=1&orderby=similarity")]
        Task<GetGameByNameResponse> GetGameByName([Path] string name);

        [Get("games/{gameId}/categories?orderby=pos")]
        Task<GetGameCategoriesResponse> GetGameCategories([Path] string gameId);
    }
}