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
    }
}
