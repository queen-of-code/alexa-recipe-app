using RecipeAPI.FirestoreModels;
using RecipeApp.Core.ExternalModels;
using System;
using System.Collections.Generic;
using Xunit;

namespace RecipeAPI.Tests
{
    [Trait("Category", "Unit")]
    public class RecipeTests
    {
        private static readonly Recipe Recipe1 = new Recipe
        {
            CookTimeMins = 1,
            Name = "Recipe1",
            PrepTimeMins = 2,
            Id = null,   // no Id — not yet persisted
            Servings = 4,
            UserId = "5"
        };

        private static readonly Recipe Recipe2 = new Recipe
        {
            CookTimeMins = 10,
            Name = "Recipe2",
            PrepTimeMins = 20,
            Id = "id-3",
            Servings = 40,
            UserId = "50"
        };

        private static readonly Recipe Recipe3 = new Recipe
        {
            CookTimeMins = 100,
            Name = "Recipe3",
            PrepTimeMins = 200,
            Id = "id-3",
            Servings = 400,
            UserId = "50"
        };

        [Fact]
        public void CopyConstructor_Copied()
        {
            var updateTime = DateTime.UtcNow;

            var external = new RecipeModel
            {
                CookTimeMins = 1,
                LastUpdateTime = updateTime,
                Name = "Some Name",
                PrepTimeMins = 2,
                RecipeId = "recipe-id-123",
                Servings = 4,
                UserId = "5"
            };
            external.Ingredients.Add("carrots");
            external.Steps.Add("do something");

            var copy = new Recipe(external);

            Assert.NotNull(copy);
            Assert.Equal(external.CookTimeMins, copy.CookTimeMins);
            Assert.Equal(external.Name, copy.Name);
            Assert.Equal(external.PrepTimeMins, copy.PrepTimeMins);
            Assert.Equal(external.RecipeId, copy.Id);
            Assert.Equal(external.Servings, copy.Servings);
            Assert.Equal(external.UserId, copy.UserId);
            Assert.Equal(external.Steps.Count, copy.Steps.Count);
            Assert.Equal(external.Ingredients.Count, copy.Ingredients.Count);

            copy.Steps.Add("profit");
            Assert.Equal(external.Steps.Count + 1, copy.Steps.Count);

            copy.Ingredients.RemoveAt(0);
            Assert.Equal(external.Ingredients.Count - 1, copy.Ingredients.Count);
        }

        [Fact]
        public void CopyConstructor_CopiesCompletedImageUrl()
        {
            var external = new RecipeModel
            {
                CookTimeMins = 1,
                LastUpdateTime = DateTime.UtcNow,
                Name = "N",
                RecipeId = "rid",
                Servings = 1,
                UserId = "u",
                CompletedImageUrl = "https://example.com/x.png",
            };
            external.Ingredients.Add("a");
            external.Steps.Add("s");

            var copy = new Recipe(external);
            Assert.Equal("https://example.com/x.png", copy.CompletedImageUrl);
        }

        [Fact]
        public void GenerateExternalRecipe_IncludesCompletedImageUrl()
        {
            var recipe = new Recipe
            {
                UserId = "u",
                Id = "id",
                Name = "N",
                CookTimeMins = 1,
                PrepTimeMins = 1,
                Servings = 1,
                CompletedImageUrl = "https://x/y",
            };
            recipe.Ingredients.Add("a");
            recipe.Steps.Add("s");

            var ext = recipe.GenerateExternalRecipe();
            Assert.Equal("https://x/y", ext.CompletedImageUrl);
        }

        [Fact]
        public void CopyConstructor_CopiesIsFavorite()
        {
            var external = new RecipeModel
            {
                CookTimeMins = 1,
                LastUpdateTime = DateTime.UtcNow,
                Name = "N",
                RecipeId = "rid",
                Servings = 1,
                UserId = "u",
                IsFavorite = true,
            };
            external.Ingredients.Add("a");
            external.Steps.Add("s");

            var copy = new Recipe(external);
            Assert.True(copy.IsFavorite);
        }

        [Fact]
        public void GenerateExternalRecipe_IncludesIsFavorite()
        {
            var recipe = new Recipe
            {
                UserId = "u",
                Id = "id",
                Name = "N",
                CookTimeMins = 1,
                PrepTimeMins = 1,
                Servings = 1,
                IsFavorite = true,
            };
            recipe.Ingredients.Add("a");
            recipe.Steps.Add("s");

            var ext = recipe.GenerateExternalRecipe();
            Assert.True(ext.IsFavorite);
        }

        public static IEnumerable<object[]> GetRecipes()
        {
            yield return new object[] { Recipe1, Recipe2, false };
            yield return new object[] { Recipe1, null, false };
            yield return new object[] { Recipe1, Recipe3, false };
            yield return new object[] { Recipe2, Recipe3, true };  // same UserId + Id
        }

        [Theory]
        [MemberData(nameof(GetRecipes))]
        public void TestEquals(Recipe recipe1, Recipe recipe2, bool shouldEqual)
        {
            Assert.Equal(shouldEqual, recipe1.Equals(recipe2));
        }

        [Fact]
        public void Recipe_StepsProperty_CanBeSetAndGet()
        {
            var recipe = new Recipe();
            var testSteps = new List<string> { "Step 1", "Step 2", "Step 3" };
            recipe.Steps = testSteps;

            Assert.NotNull(recipe.Steps);
            Assert.Equal(3, recipe.Steps.Count);
            Assert.Equal("Step 1", recipe.Steps[0]);
        }

        [Fact]
        public void Recipe_ConstructorFromRecipeModel_CopiesStepsCorrectly()
        {
            var recipeModel = new RecipeModel
            {
                UserId = "test-user",
                RecipeId = "model-456",
                Name = "Model Recipe",
                CookTimeMins = 25,
                PrepTimeMins = 10,
                Servings = 2,
                LastUpdateTime = DateTime.UtcNow
            };
            recipeModel.Steps.AddRange(new[] { "Step A", "Step B", "Step C" });
            recipeModel.Ingredients.AddRange(new[] { "Ingredient 1", "Ingredient 2" });

            var recipe = new Recipe(recipeModel);

            Assert.Equal(recipeModel.Steps.Count, recipe.Steps.Count);
            Assert.Equal("Step A", recipe.Steps[0]);

            recipe.Steps.Add("Step D");
            Assert.Equal(3, recipeModel.Steps.Count);
            Assert.Equal(4, recipe.Steps.Count);
        }

        [Fact]
        public void Recipe_GenerateExternalRecipe_CopiesStepsCorrectly()
        {
            var recipe = new Recipe
            {
                UserId = "test-user",
                Id = "internal-789",
                Name = "Internal Recipe",
                CookTimeMins = 45,
                PrepTimeMins = 20,
                Servings = 6,
            };
            recipe.Steps.AddRange(new[] { "Internal Step 1", "Internal Step 2" });
            recipe.Ingredients.AddRange(new[] { "Internal Ingredient 1" });

            var externalRecipe = recipe.GenerateExternalRecipe();

            Assert.Equal(recipe.Steps.Count, externalRecipe.Steps.Count);
            Assert.Equal("Internal Step 1", externalRecipe.Steps[0]);
            Assert.Equal(recipe.Id, externalRecipe.RecipeId);

            externalRecipe.Steps.Add("External Step 3");
            Assert.Equal(2, recipe.Steps.Count);
            Assert.Equal(3, externalRecipe.Steps.Count);
        }
    }
}
