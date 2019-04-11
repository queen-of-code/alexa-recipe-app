
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using RecipeApp.Core;
using RecipeApp.Core.ExternalModels;

namespace RecipeApp.API.DynamoModels
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

        /// <summary>
        /// Converts the internal database format of our recipe object into the external
        /// format that is meant to be consumed by the UI or over the API.
        /// </summary>
        public RecipeModel GenerateExternalRecipe()
        {
            var recipe = new RecipeModel();
            recipe.UserId = this.UserId;
            recipe.RecipeId = this.RecipeId;
            recipe.Name = this.Name;
            recipe.LastUpdateTime = this.LastUpdateTime;
            recipe.PrepTimeMins = this.PrepTimeMins;
            recipe.CookTimeMins = this.CookTimeMins;
            recipe.Servings = this.Servings;
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

        private static readonly AmazonDynamoDBClient client = new AmazonDynamoDBClient(RegionEndpoint.USWest2);

        /// <summary>
        /// Retrieves a recipe from the database. Returns null if nothing could be found.
        /// </summary>
        public static async Task<Recipe> RetrieveRecipe(string userId, long recipeId)
        {
            var context = new DynamoDBContext(client);

            try
            {
                return await context.LoadAsync<Recipe>(userId, recipeId);
            }
            catch (AmazonDynamoDBException)
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes a recipe from the database. Returns false if something went wrong, or
        /// true if it was deleted successfully.
        /// </summary>
        public static async Task<bool> DeleteRecipe(string userId, long recipeId)
        {
            var recipe = await RetrieveRecipe(userId, recipeId);

            return await DeleteRecipe(recipe);
        }

        /// <summary>
        /// Deletes a recipe from the database. Returns false if something went wrong, or
        /// true if it was deleted successfully.
        /// </summary>
        public static async Task<bool> DeleteRecipe(Recipe recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            var context = new DynamoDBContext(client);

            try
            {
                await context.DeleteAsync(recipe);
                return true;
            }
            catch (AmazonDynamoDBException)
            {
                return false;
            }
        }

        /// <summary>
        /// Saves a recipe into the database. Returns false if something went wrong, or
        /// true if it was saved successfully.
        /// </summary>
        public static async Task<bool> SaveRecipe(Recipe recipe)
        {
            if (recipe == null || !recipe.IsValid())
            {
                return false;
            }

            var context = new DynamoDBContext(client);

            recipe.LastUpdateTime = DateTime.UtcNow;
            if (recipe.RecipeId == default(long))
            {
                recipe.RecipeId = Utilities.NextInt64();
            }

            try
            {
                await context.SaveAsync(recipe);
                return true;
            }
            catch (AmazonDynamoDBException)
            {
                return false;
            }
        }
    }
}
