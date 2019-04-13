using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RecipeApp.Core.ExternalModels
{
    public class RecipeModel
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("recipe_id")]
        public long RecipeId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("last_updated")]
        public DateTime LastUpdateTime { get; set; }

        [NotMapped]
        [JsonProperty("ingredients")]
        public List<string> Ingredients { get; private set; } = new List<string>();

        [NotMapped]
        [JsonProperty("steps")]
        public List<string> Steps { get; private set; } = new List<string>();

        [JsonProperty("servings")]
        public int Servings { get; set; }

        [JsonProperty("prep_minutes")]
        public int PrepTimeMins { get; set; }

        [JsonProperty("cook_minutes")]
        public int CookTimeMins { get; set; }
    }
}
