using System;
using System.Collections.Generic;
using System.Linq;

using Google.Cloud.Firestore;

using RecipeAPI.FirestoreModels;
using RecipeApp.Core.ExternalModels;
using Xunit;

namespace RecipeAPI.Tests
{
    [Trait("Category", "Unit")]
    public class RecipeListOrderingTests
    {
        private static Recipe Recipe(string id, bool isFavorite, DateTime lastUpdated)
        {
            var r = new Recipe
            {
                Id = id,
                UserId = "u1",
                Name = id,
                CookTimeMins = 1,
                PrepTimeMins = 1,
                Servings = 1,
                IsFavorite = isFavorite,
                LastUpdateTime = Timestamp.FromDateTime(DateTime.SpecifyKind(lastUpdated, DateTimeKind.Utc)),
            };
            r.Ingredients.Add("a");
            r.Steps.Add("s");
            return r;
        }

        [Fact]
        public void Apply_FavoritesFirst_ThenLastUpdatedDesc()
        {
            var olderFavorite = Recipe("f-old", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var newerFavorite = Recipe("f-new", true, new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));
            var newerNonFavorite = Recipe("n-new", false, new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc));
            var olderNonFavorite = Recipe("n-old", false, new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc));

            var ordered = RecipeListOrdering.Apply(
                new List<Recipe> { olderNonFavorite, newerNonFavorite, olderFavorite, newerFavorite },
                favoritesOnly: false).Select(r => r.RecipeId).ToList();

            Assert.Equal(new[] { "f-new", "f-old", "n-new", "n-old" }, ordered);
        }

        [Fact]
        public void Apply_FavoritesOnly_FiltersAndSortsByLastUpdatedDesc()
        {
            var older = Recipe("f-old", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var newer = Recipe("f-new", true, new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));
            var notFavorite = Recipe("n", false, new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc));

            var ordered = RecipeListOrdering.Apply(
                new List<Recipe> { older, notFavorite, newer },
                favoritesOnly: true).Select(r => r.RecipeId).ToList();

            Assert.Equal(new[] { "f-new", "f-old" }, ordered);
        }

        [Fact]
        public void Apply_DefaultsMissingIsFavoriteToFalse()
        {
            var recipe = Recipe("r1", false, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var model = recipe.GenerateExternalRecipe();
            Assert.False(model.IsFavorite);
        }
    }
}
