using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RecipeAPI
{
    public class RecipeSearchRequest
    {
        [JsonPropertyName("ingredients")]
        public List<string> Ingredients { get; set; }

        [JsonPropertyName("combine")]
        public string Combine { get; set; }

        [JsonPropertyName("query")]
        public string Query { get; set; }
    }
}
