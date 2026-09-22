using System.Collections.Generic;
using System.Threading.Tasks;

using RecipeAPI.FirestoreModels;

namespace RecipeAPI
{
    public interface IFirestoreRecipeService
    {
        Task<bool> DeleteRecipe(string userId, string recipeId);
        Task<IEnumerable<Recipe>> GetAllRecipesForUser(string userId);
        Task<Recipe> RetrieveRecipe(string userId, string recipeId);

        /// <summary>
        /// Sets or clears <see cref="Recipe.IsFavorite"/> and saves through <see cref="SaveRecipe"/>.
        /// Idempotent when the flag already matches <paramref name="isFavorite"/>.
        /// </summary>
        Task<FavoriteUpdateResult> SetFavorite(string userId, string recipeId, bool isFavorite);
        /// <summary>Returns the saved recipe (with assigned id and timestamps) on success, or null on failure.</summary>
        Task<Recipe> SaveRecipe(Recipe recipe);

        Task<T> RetrieveItem<T>(string userId, string itemId) where T : class, IFirestoreEntity, new();
        Task<bool> SaveItem<T>(T item) where T : class, IFirestoreEntity, new();
        Task<bool> DeleteItem<T>(string userId, string itemId) where T : class, IFirestoreEntity, new();
        Task<IEnumerable<T>> GetAllItemsForUser<T>(string userId) where T : class, IFirestoreEntity, new();
    }

    /// <summary>
    /// Outcome of <see cref="IFirestoreRecipeService.SetFavorite"/>.
    /// <see cref="Found"/> is false when the recipe does not exist.
    /// When the recipe exists but the save fails, <see cref="Found"/> is true and <see cref="Recipe"/> is null.
    /// </summary>
    public sealed class FavoriteUpdateResult
    {
        public FavoriteUpdateResult(bool found, Recipe recipe)
        {
            Found = found;
            Recipe = recipe;
        }

        public bool Found { get; }

        public Recipe Recipe { get; }
    }
}
