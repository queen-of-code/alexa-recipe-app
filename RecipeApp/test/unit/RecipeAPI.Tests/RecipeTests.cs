using RecipeAPI.DynamoModels;
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
            LastUpdateTime = DateTime.UtcNow,
            Name = "Receip1",
            PrepTimeMins = 2,
            EntityId = 0,
            Servings = 4,
            UserId = "5"
        };

        private static readonly Recipe Recipe2 = new Recipe
        {
            CookTimeMins = 10,
            LastUpdateTime = DateTime.Today,
            Name = "Receip2",
            PrepTimeMins = 20,
            EntityId = 3,
            Servings = 40,
            UserId = "50"
        };

        private static readonly Recipe Recipe3 = new Recipe
        {
            CookTimeMins = 100,
            LastUpdateTime = DateTime.MinValue,
            Name = "Recipe3",
            PrepTimeMins = 200,
            EntityId = 3,
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
                Name = "SOme Name",
                PrepTimeMins = 2,
                RecipeId = 3,
                Servings = 4,
                UserId = "5"
            };
            external.Ingredients.Add("carrots");
            external.Steps.Add("do something");

            var copy = new Recipe(external);

            Assert.NotNull(copy);
            Assert.Equal(external.CookTimeMins, copy.CookTimeMins);
            Assert.Equal(external.LastUpdateTime, copy.LastUpdateTime);
            Assert.Equal(external.Name, copy.Name);
            Assert.Equal(external.PrepTimeMins, copy.PrepTimeMins);
            Assert.Equal(external.RecipeId, copy.EntityId);
            Assert.Equal(external.Servings, copy.Servings);
            Assert.Equal(external.UserId, copy.UserId);
            Assert.Equal(external.Steps.Count, copy.Steps.Count);
            Assert.Equal(external.Ingredients.Count, copy.Ingredients.Count);

            copy.Steps.Add("profit");
            Assert.Equal(external.Steps.Count + 1, copy.Steps.Count);

            copy.Ingredients.RemoveAt(0);
            Assert.Equal(external.Ingredients.Count - 1, copy.Ingredients.Count);
        }

        /// <summary>
        /// Test data for the function below it.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> GetRecipes()
        {
            yield return new object[] { Recipe1, Recipe2, false };
            yield return new object[] { Recipe1, null, false };
            yield return new object[] { Recipe1, Recipe3, false };
            yield return new object[] { Recipe2, Recipe3, true };
        }

        [Theory]
        [MemberData(nameof(GetRecipes))]
        public void TestEquals(Recipe recipe1, Recipe recipe2, bool shouldEqual)
        {
            var result = recipe1.Equals(recipe2);
            Assert.Equal(shouldEqual, result);
        }

        [Fact]
        public void Recipe_StepsProperty_CanBeSetAndGet()
        {
            // Arrange
            var recipe = new Recipe();
            var testSteps = new List<string> { "Step 1", "Step 2", "Step 3" };

            // Act
            recipe.Steps = testSteps;

            // Assert
            Assert.NotNull(recipe.Steps);
            Assert.Equal(3, recipe.Steps.Count);
            Assert.Equal("Step 1", recipe.Steps[0]);
            Assert.Equal("Step 2", recipe.Steps[1]);
            Assert.Equal("Step 3", recipe.Steps[2]);
        }

        [Fact]
        public void Recipe_JsonSerialization_PreservesSteps()
        {
            // Arrange
            var originalRecipe = new Recipe
            {
                UserId = "test-user",
                EntityId = 123,
                Name = "Test Recipe",
                CookTimeMins = 30,
                PrepTimeMins = 15,
                Servings = 4,
                LastUpdateTime = DateTime.UtcNow
            };
            originalRecipe.Steps.AddRange(new[] { "Preheat oven", "Mix ingredients", "Bake for 30 minutes" });
            originalRecipe.Ingredients.AddRange(new[] { "Flour", "Sugar", "Eggs" });

            // Act - Serialize to JSON and back
            var json = System.Text.Json.JsonSerializer.Serialize(originalRecipe);
            var deserializedRecipe = System.Text.Json.JsonSerializer.Deserialize<Recipe>(json);

            // Assert
            Assert.NotNull(deserializedRecipe);
            Assert.Equal(originalRecipe.Steps.Count, deserializedRecipe.Steps.Count);
            Assert.Equal(originalRecipe.Steps[0], deserializedRecipe.Steps[0]);
            Assert.Equal(originalRecipe.Steps[1], deserializedRecipe.Steps[1]);
            Assert.Equal(originalRecipe.Steps[2], deserializedRecipe.Steps[2]);
        }

        [Fact]
        public void Recipe_ConstructorFromRecipeModel_CopiesStepsCorrectly()
        {
            // Arrange
            var recipeModel = new RecipeModel
            {
                UserId = "test-user",
                RecipeId = 456,
                Name = "Model Recipe",
                CookTimeMins = 25,
                PrepTimeMins = 10,
                Servings = 2,
                LastUpdateTime = DateTime.UtcNow
            };
            recipeModel.Steps.AddRange(new[] { "Step A", "Step B", "Step C" });
            recipeModel.Ingredients.AddRange(new[] { "Ingredient 1", "Ingredient 2" });

            // Act
            var recipe = new Recipe(recipeModel);

            // Assert
            Assert.NotNull(recipe.Steps);
            Assert.Equal(recipeModel.Steps.Count, recipe.Steps.Count);
            Assert.Equal("Step A", recipe.Steps[0]);
            Assert.Equal("Step B", recipe.Steps[1]);
            Assert.Equal("Step C", recipe.Steps[2]);
            
            // Verify they are independent lists
            recipe.Steps.Add("Step D");
            Assert.Equal(3, recipeModel.Steps.Count); // Original should be unchanged
            Assert.Equal(4, recipe.Steps.Count); // New should have 4
        }

        [Fact]
        public void Recipe_GenerateExternalRecipe_CopiesStepsCorrectly()
        {
            // Arrange
            var recipe = new Recipe
            {
                UserId = "test-user",
                EntityId = 789,
                Name = "Internal Recipe",
                CookTimeMins = 45,
                PrepTimeMins = 20,
                Servings = 6,
                LastUpdateTime = DateTime.UtcNow
            };
            recipe.Steps.AddRange(new[] { "Internal Step 1", "Internal Step 2" });
            recipe.Ingredients.AddRange(new[] { "Internal Ingredient 1" });

            // Act
            var externalRecipe = recipe.GenerateExternalRecipe();

            // Assert
            Assert.NotNull(externalRecipe.Steps);
            Assert.Equal(recipe.Steps.Count, externalRecipe.Steps.Count);
            Assert.Equal("Internal Step 1", externalRecipe.Steps[0]);
            Assert.Equal("Internal Step 2", externalRecipe.Steps[1]);
            
            // Verify they are independent lists
            externalRecipe.Steps.Add("External Step 3");
            Assert.Equal(2, recipe.Steps.Count); // Original should be unchanged
            Assert.Equal(3, externalRecipe.Steps.Count); // New should have 3
        }
    }
}
