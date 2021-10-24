using System.Threading.Tasks;
using RestEase;
using TwitchRestEase.Models;

namespace TwitchRestEase
{
    [BasePath("helix")]
    [Header("User-Agent", "RestEase")]
    public interface ITwitchApi
    {
        [Header("Client-Id")]
        string ClientId { get; set; }

        [Header("Authorization")]
        string Authorization { get; set; }

        [Get("streams?user_login={userLogin}")]
        Task<Streams> GetStreams([Path] string userLogin);
    }
}