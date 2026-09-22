using Google.Cloud.Firestore;
using RecipeAPI.FirestoreModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RecipeAPI.Tests
{
    [Trait("Category", "Unit")]
    public class RecipeListOrderingTests
    {
        private static Recipe RecipeAt(string id, bool isFavorite, DateTime updatedUtc)
        {
            return new Recipe
            {
                Id = id,
                UserId = "user",
                Name = id,
                IsFavorite = isFavorite,
                LastUpdateTime = Timestamp.FromDateTime(DateTime.SpecifyKind(updatedUtc, DateTimeKind.Utc)),
            };
        }

        [Fact]
        public void OrderForList_PutsEveryFavoriteBeforeNonFavorites()
        {
            var olderFavorite = RecipeAt("fav-old", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var newerPlain = RecipeAt("plain-new", false, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var ordered = RecipeListOrdering.OrderForList(
                new List<Recipe> { newerPlain, olderFavorite },
                favoritesOnly: false);

            Assert.Equal(new[] { "fav-old", "plain-new" }, ordered.Select(r => r.Id).ToArray());
        }

        [Fact]
        public void OrderForList_SortsWithinGroupByLastUpdateDescending()
        {
            var older = RecipeAt("older", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var newer = RecipeAt("newer", true, new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc));

            var ordered = RecipeListOrdering.OrderForList(
                new List<Recipe> { older, newer },
                favoritesOnly: false);

            Assert.Equal(new[] { "newer", "older" }, ordered.Select(r => r.Id).ToArray());
        }

        [Fact]
        public void OrderForList_FavoritesOnly_DropsNonFavorites()
        {
            var favorite = RecipeAt("fav", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var plain = RecipeAt("plain", false, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var ordered = RecipeListOrdering.OrderForList(
                new List<Recipe> { plain, favorite },
                favoritesOnly: true);

            Assert.Equal(new[] { "fav" }, ordered.Select(r => r.Id).ToArray());
        }

        [Fact]
        public void OrderForList_TreatsDefaultIsFavoriteAsNotFavorite()
        {
            var unset = new Recipe
            {
                Id = "unset",
                UserId = "user",
                Name = "unset",
                LastUpdateTime = Timestamp.FromDateTime(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            };
            var favorite = RecipeAt("fav", true, new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var ordered = RecipeListOrdering.OrderForList(
                new List<Recipe> { unset, favorite },
                favoritesOnly: false);

            Assert.Equal(new[] { "fav", "unset" }, ordered.Select(r => r.Id).ToArray());
        }
    }
}
