using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using RecipeAPI.DynamoModels;
using RecipeAPI;
using Amazon.DynamoDBv2;

namespace RecipeApp.API.Tests
{
    public class RecipeModelTests
    {
        [Fact]
        [Trait("Category","Integration")]
        public async void TestSave_Create_Delete()
        {
            var moq = new Moq.Mock<IAmazonDynamoDB>();
            var recipeService = new DynamoRecipeService(moq.Object);

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
                var saved = await recipeService.SaveRecipe(recipe);
;                Assert.True(saved);

                var retrieved = await recipeService.RetrieveRecipe(recipe.UserId, recipe.RecipeId);
                var equal = recipe.Equals(retrieved);
                Assert.True(equal);
            }
            finally
            {
                var deleted = await recipeService.DeleteRecipe(recipe);
                Assert.True(deleted);
            }
        }
    }
}
