using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeAPI.DynamoModels;

namespace RecipeAPI
{
    public interface IDynamoRecipeService
    {
        Task<bool> DeleteRecipe(string userId, long recipeId);
        Task<IEnumerable<Recipe>> GetAllRecipesForUser(string userId);
        Task<Recipe> RetrieveRecipe(string userId, long recipeId);
        Task<bool> SaveRecipe(Recipe recipe);
    }
}