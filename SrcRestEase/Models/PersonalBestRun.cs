using Newtonsoft.Json;

namespace SrcRestEase.Models
{
    public class PersonalBestRun
    {
        [JsonProperty("category")]
        public string CategoryId { get; set; }

        public Times Times { get; set; }
    }
}