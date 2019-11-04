using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Moq;
using RecipeAPI.DynamoModels;
using Xunit;

namespace RecipeAPI.Tests
{
    [Trait("Category", "Unit")]
    public class DynamoRecipeServiceTests
    {


        [Fact]
        public async Task EnsureTableExists_True()
        {
            var moq = new Mock<IAmazonDynamoDB>();
            moq.SetupAllProperties();
            moq.Setup(s => 
                s.ListTablesAsync(It.IsAny<ListTablesRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ListTablesResponse { TableNames = new List<string> { "Recipe" } });

            var exists = await DynamoRecipeService.EnsureTableExists(moq.Object);
            Assert.True(exists);
        }

        [Fact]
        public async Task EnsureTableExists_False()
        {
            var moq = new Mock<IAmazonDynamoDB>();
            moq.SetupAllProperties();
            moq.Setup(s =>
                s.ListTablesAsync(It.IsAny<ListTablesRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ListTablesResponse { TableNames = new List<string> { "NOTHING" } });
            moq.Setup(s =>
                s.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new CreateTableResponse { HttpStatusCode = HttpStatusCode.BadRequest });

            var exists = await DynamoRecipeService.EnsureTableExists(moq.Object);
            Assert.False(exists);
        }

        [Fact]
        public async Task EnsureTableExists_Created()
        {
            var moq = new Mock<IAmazonDynamoDB>();
            moq.SetupAllProperties();
            moq.Setup(s =>
                s.ListTablesAsync(It.IsAny<ListTablesRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ListTablesResponse { TableNames = new List<string> { "NOTHING" } });
            moq.Setup(s =>
                s.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new CreateTableResponse { HttpStatusCode = HttpStatusCode.OK });

            var exists = await DynamoRecipeService.EnsureTableExists(moq.Object);
            Assert.True(exists);
        }

        [Fact]
        public async Task SaveRecipe_Valid()
        {
            var moq = new Mock<IDynamoDBContext>();
            Recipe callback = null;

            moq.SetupAllProperties();
            moq.Setup(s =>
                s.SaveAsync(It.IsAny<Recipe>(), It.IsAny<CancellationToken>()))
                .Callback((Recipe r, CancellationToken ct) => { callback = r; });

            var recipeService = new DynamoRecipeService(null);
            DynamoRecipeService.Initialized = true;
            recipeService.SetupMockContext(moq.Object);

            var recipe = new Recipe
            {
                Name = "Test Recipe",
                LastUpdateTime = DateTime.UtcNow,
                UserId = "123",
                RecipeId = 456,
                CookTimeMins = 11,
                PrepTimeMins = 22,
                Servings = 99
            };

            var result = await recipeService.SaveRecipe(recipe);
            Assert.True(result);
            Assert.NotNull(callback);
            Assert.Equal(recipe.RecipeId, callback.RecipeId);
        }

        [Fact]
        public async Task SaveRecipe_NoId()
        {
            var moq = new Mock<IDynamoDBContext>();
            Recipe callback = null;

            moq.SetupAllProperties();
            moq.Setup(s =>
                s.SaveAsync(It.IsAny<Recipe>(), It.IsAny<CancellationToken>()))
                .Callback((Recipe r, CancellationToken ct) => { callback = r; });

            var recipeService = new DynamoRecipeService(null);
            DynamoRecipeService.Initialized = true;
            recipeService.SetupMockContext(moq.Object);

            var recipe = new Recipe
            {
                Name = "Test Recipe",
                LastUpdateTime = DateTime.UtcNow,
                UserId = "123",
                CookTimeMins = 11,
                PrepTimeMins = 22,
                Servings = 99
            };

            var result = await recipeService.SaveRecipe(recipe);
            Assert.True(result);
            Assert.NotNull(callback);
            Assert.NotEqual(default, callback.RecipeId);
        }

        [Fact]
        public async Task SaveRecipe_Invalid()
        {
            // TODO Also mock the Client because you need it!
            var moq = new Mock<IDynamoDBContext>();
            Recipe callback = null;

            moq.SetupAllProperties();
            moq.Setup(s =>
                s.SaveAsync(It.IsAny<Recipe>(), It.IsAny<CancellationToken>()))
                .Callback((Recipe r, CancellationToken ct) => { callback = r; });

            var recipeService = new DynamoRecipeService(null);
            DynamoRecipeService.Initialized = true;
            recipeService.SetupMockContext(moq.Object);

            var recipe = new Recipe
            {
                Name = "",
                LastUpdateTime = DateTime.UtcNow,
                UserId = "123",
                RecipeId = 456,
                CookTimeMins = 11,
                PrepTimeMins = 22,
                Servings = 99
            };

            var result = await recipeService.SaveRecipe(recipe);
            Assert.False(result);
        }

    }
}
