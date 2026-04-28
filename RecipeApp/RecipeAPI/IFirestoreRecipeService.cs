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
        /// <summary>Returns the saved recipe (with assigned id and timestamps) on success, or null on failure.</summary>
        Task<Recipe> SaveRecipe(Recipe recipe);

        Task<T> RetrieveItem<T>(string userId, string itemId) where T : class, IFirestoreEntity, new();
        Task<bool> SaveItem<T>(T item) where T : class, IFirestoreEntity, new();
        Task<bool> DeleteItem<T>(string userId, string itemId) where T : class, IFirestoreEntity, new();
        Task<IEnumerable<T>> GetAllItemsForUser<T>(string userId) where T : class, IFirestoreEntity, new();
    }
}
