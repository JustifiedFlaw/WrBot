using Newtonsoft.Json;

namespace TwitchRestEase.Models
{
    public class Stream
    {
        [JsonProperty("game_name")]
        public string GameName { get; set; }

        public string Title { get; set; }
    }
}