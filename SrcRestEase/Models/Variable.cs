using Newtonsoft.Json;

namespace SrcRestEase.Models
{
    public class Variable
    {
        public string Id { get; set; }

        [JsonProperty("is-subcategory")]
        public bool IsSubCategory { get; set; }

        public VariableValues Values { get; set; }
    }
}