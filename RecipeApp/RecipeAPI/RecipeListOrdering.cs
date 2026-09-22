using System.Collections.Generic;
using System.Linq;

using RecipeAPI.FirestoreModels;

namespace RecipeAPI
{
    /// <summary>
    /// Favorites-first list ordering from the recipe-favoriting Tech Spec.
    /// Within each favorite group, <see cref="Recipe.LastUpdateTime"/> descending.
    /// </summary>
    public static class RecipeListOrdering
    {
        public static IReadOnlyList<Recipe> OrderForList(IEnumerable<Recipe> recipes, bool favoritesOnly)
        {
            var query = recipes ?? new List<Recipe>();
            if (favoritesOnly)
                query = query.Where(r => r.IsFavorite);

            return query
                .OrderByDescending(r => r.IsFavorite)
                .ThenByDescending(r => r.LastUpdateTime)
                .ToList();
        }
    }
}
