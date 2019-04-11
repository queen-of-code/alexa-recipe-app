using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using RecipeApp.API;
using RecipeApp.API.DynamoModels;

namespace RecipeApp.API.Tests
{
    public class RecipeModelTests
    {
        [Fact]
        [Trait("Category","Integration")]
        public async void TestSave_Create_Delete()
        {
            var recipe = new Recipe()
            {
                Name = "Test Recipe",
                LastUpdateTime = DateTime.UtcNow,
                UserId = "123",
                RecipeId = 456,
                CookTimeMins = 11,
                PrepTimeMins = 22,
                Servings = 99
            };

            try
            {
                var saved = await Recipe.SaveRecipe(recipe);
                Assert.True(saved);

                var retrieved = await Recipe.RetrieveRecipe(recipe.UserId, recipe.RecipeId);
                var equal = recipe.Equals(retrieved);
                Assert.True(equal);
            }
            finally
            {
                var deleted = await Recipe.DeleteRecipe(recipe);
                Assert.True(deleted);
            }
        }
    }
}
