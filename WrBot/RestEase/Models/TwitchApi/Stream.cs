using Newtonsoft.Json;

namespace RestEase.Models.TwitchApi
{
    public class Stream
    {
        [JsonProperty("game_name")]
        public string GameName { get; set; }

        public string Title { get; set; }
    }
}