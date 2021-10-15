using System.Threading.Tasks;
using RestEase.Models.TwitchApi;

namespace RestEase.Interfaces
{
    [BasePath("helix")]
    [Header("User-Agent", "RestEase")]
    public interface ITwitchApi
    {
        [Header("Client-Id")]
        public string ClientId { get; set; }

        [Header("Authorization")]
        public string Authorization { get; set; }

        [Get("streams?user_login={userLogin}")]
        Task<Streams> GetStreams([Path] string userLogin);
    }
}