using Amazon.DynamoDBv2.DataModel;
using Moq;
using RecipeAPI.DynamoModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UnitTestBase;

namespace RecipeAPI.Tests
{
    [Trait("Category", "Unit")]
    public class DynamoRecipeServiceTests : TestBase
    {
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
                EntityId = 456,
                CookTimeMins = 11,
                PrepTimeMins = 22,
                Servings = 99
            };

            var result = await recipeService.SaveRecipe(recipe);
            Assert.True(result);
            Assert.NotNull(callback);
            Assert.Equal(recipe.EntityId, callback.EntityId);
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
            Assert.NotEqual(default, callback.EntityId);
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
                EntityId = 456,
                CookTimeMins = 11,
                PrepTimeMins = 22,
                Servings = 99
            };

            var result = await recipeService.SaveRecipe(recipe);
            Assert.False(result);
        }

        [Fact]
        public async Task SaveItem_Valid()
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
                EntityId = 456,
                CookTimeMins = 11,
                PrepTimeMins = 22,
                Servings = 99
            };

            var result = await recipeService.SaveItem<Recipe>(recipe);
            Assert.True(result);
            Assert.NotNull(callback);
            Assert.Equal(recipe.EntityId, callback.EntityId);
        }

        [Fact]
        public async Task SaveItem_NoId()
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

            var result = await recipeService.SaveItem(recipe);
            Assert.True(result);
            Assert.NotNull(callback);
            Assert.NotEqual(default, callback.EntityId);
        }

        [Fact]
        public async Task SaveItem_Invalid()
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
                EntityId = 456,
                CookTimeMins = 11,
                PrepTimeMins = 22,
                Servings = 99
            };

            var result = await recipeService.SaveItem(recipe);
            Assert.False(result);
        }

    }
}
