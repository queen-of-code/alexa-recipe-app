using System.Text.Json.Serialization;

namespace RecipeAPI
{
    public class FavoriteRequest
    {
        [JsonPropertyName("favorite")]
        public bool? Favorite { get; set; }
    }
}
