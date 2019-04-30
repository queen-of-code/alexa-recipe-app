using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using RecipeAPI.DynamoModels;
using RecipeAPI;
using Amazon.DynamoDBv2;
using Moq;
using Amazon.DynamoDBv2.Model;
using System.Threading;

namespace RecipeAPI.Tests
{
    public class DynamoRecipeServiceTests
    {
        [Fact]
        public async void TestSave_Create_Delete()
        {
            var moq = new Moq.Mock<IAmazonDynamoDB>();
            moq.SetupAllProperties();
            moq.Setup(s =>
                s.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteItemResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

            moq.Setup(s =>
                s.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutItemResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

            moq.Setup(s =>
                s.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QueryResponse { HttpStatusCode = System.Net.HttpStatusCode.OK, });

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
                Assert.True(saved);

                //var retrieved = await recipeService.RetrieveRecipe(recipe.UserId, recipe.RecipeId);
                //var equal = recipe.Equals(retrieved);
                //Assert.True(equal);
            }
            finally
            {
               //var deleted = await recipeService.DeleteRecipe(recipe);
               //Assert.True(deleted);
            }
        }
    }
}
