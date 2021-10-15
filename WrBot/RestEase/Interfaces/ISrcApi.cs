using System.Threading.Tasks;
using RestEase.Models.Src;
using RestEase.Models.TwitchApi;

namespace RestEase.Interfaces
{
    [BasePath("v1")]
    [Header("User-Agent", "RestEase")]
    [Header("Authorization", "Bearer txeqhs9wab81yv9hwl84mh1rw8rg4j")]
    [Header("Client-Id", "a6423urau9t5wwb06seg4pj6ptm3q3")]
    public interface ISrcApi
    {
        [Get("leaderboards/{gameId}/category/{categoryId}?top=1")]
        Task<GetLeaderboardResponse> GetLeaderboard([Path] string gameId, [Path] string categoryId);

        [Get("users/{userId}")]
        Task<GetUserResponse> GetUser([Path] string userId);

        [Get("games?name={name}&_bulk=yes&max=1&orderby=similarity")]
        Task<GetGameByNameResponse> GetGameByName([Path] string name);

        [Get("games/{gameId}/categories")]
        Task<GetGameCategoriesResponse> GetGameCategories([Path] string gameId);
    }
}