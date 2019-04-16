using Amazon.DynamoDBv2.DataModel;
using RecipeApp.Core.ExternalModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecipeAPI.DynamoModels
{
    [DynamoDBTable("Recipe")]
    public sealed class Recipe : IEquatable<Recipe>
    {
        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBRangeKey]
        public long RecipeId { get; set; }

        [DynamoDBProperty]
        public string Name { get; set; }

        [DynamoDBProperty]
        public DateTime LastUpdateTime { get; set; }

        [DynamoDBProperty]
        public List<string> Ingredients { get; private set; } = new List<string>();

        [DynamoDBProperty]
        public List<string> Steps { get; private set; } = new List<string>();

        [DynamoDBProperty]
        public int Servings { get; set; }

        [DynamoDBProperty]
        public int PrepTimeMins { get; set; }

        [DynamoDBProperty]
        public int CookTimeMins { get; set; }

        [DynamoDBVersion]
        public int? VersionNumber { get; set; }

        public Recipe()
        {
        }

        public Recipe(RecipeModel external)
        {
            if (external != null)
            {
                this.CookTimeMins = external.CookTimeMins;
                this.LastUpdateTime = external.LastUpdateTime;
                this.Name = external.Name;
                this.PrepTimeMins = external.PrepTimeMins;
                this.RecipeId = external.RecipeId;
                this.Servings = external.Servings;
                this.UserId = external.UserId;

                this.Steps = new List<string>(external.Steps);
                this.Ingredients = new List<string>(external.Ingredients);
            }
        }

        /// <summary>
        /// Converts the internal database format of our recipe object into the external
        /// format that is meant to be consumed by the UI or over the API.
        /// </summary>
        public RecipeModel GenerateExternalRecipe()
        {
            var recipe = new RecipeModel
            {
                UserId = this.UserId,
                RecipeId = this.RecipeId,
                Name = this.Name,
                LastUpdateTime = this.LastUpdateTime,
                PrepTimeMins = this.PrepTimeMins,
                CookTimeMins = this.CookTimeMins,
                Servings = this.Servings
            };
            recipe.Steps.AddRange(this.Steps.Select(s => s));
            recipe.Ingredients.AddRange(this.Ingredients.Select(s => s));

            return recipe;
        }

        public bool IsValid()
        {
            if (String.IsNullOrWhiteSpace(this.UserId) || this.RecipeId == default(long))
            {
                return false;
            }

            if (String.IsNullOrWhiteSpace(this.Name))
            {
                return false;
            }

            if (this.Ingredients == null || this.Steps == null)
            {
                return false;
            }

            return true;
        }

        public bool Equals(Recipe other)
        {
            if (other == null)
            {
                return false;
            }

            return this.UserId == other.UserId &&
                   this.RecipeId == other.RecipeId &&
                   this.VersionNumber == other.VersionNumber;
        }
    }
}
