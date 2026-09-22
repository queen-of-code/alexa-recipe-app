using System.Collections.Generic;
using System.Linq;

using RecipeAPI.FirestoreModels;
using RecipeApp.Core.ExternalModels;

namespace RecipeAPI
{
    internal static class RecipeListOrdering
    {
        public static IEnumerable<RecipeModel> Apply(IEnumerable<Recipe> recipes, bool favoritesOnly)
        {
            var models = recipes?.Select(r => r.GenerateExternalRecipe()) ?? Enumerable.Empty<RecipeModel>();

            if (favoritesOnly)
            {
                return models
                    .Where(r => r.IsFavorite)
                    .OrderByDescending(r => r.LastUpdateTime);
            }

            return models
                .OrderByDescending(r => r.IsFavorite)
                .ThenByDescending(r => r.LastUpdateTime);
        }
    }
}
