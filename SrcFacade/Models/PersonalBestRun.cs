using System.Collections.Generic;
using Newtonsoft.Json;

namespace SrcFacade.Models
{
    public class PersonalBestRun
    {
        [JsonProperty("category")]
        public string CategoryId { get; set; }

        public Times Times { get; set; }

        public Dictionary<string,string> Values { get; set; }
    }
}