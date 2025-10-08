using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace RecipeApp.Core.ExternalModels
{
    public class RecipeModel
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("recipeId")]
        public long RecipeId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdateTime { get; set; }

        [NotMapped]
        [JsonPropertyName("ingredients")]
        public List<string> Ingredients { get; set; } = new List<string>();

        [NotMapped]
        [JsonPropertyName("steps")]
        public List<string> Steps { get; set; } = new List<string>();

        [JsonPropertyName("servings")]
        public int Servings { get; set; }

        [JsonPropertyName("prepTimeMins")]
        public int PrepTimeMins { get; set; }

        [JsonPropertyName("cookTimeMins")]
        public int CookTimeMins { get; set; }
    }
}
