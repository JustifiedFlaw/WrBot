using Newtonsoft.Json;

namespace RestEase.Models.Src
{
    public class PersonalBestRun
    {
        [JsonProperty("category")]
        public string CategoryId { get; set; }

        public Times Times { get; set; }
    }
}