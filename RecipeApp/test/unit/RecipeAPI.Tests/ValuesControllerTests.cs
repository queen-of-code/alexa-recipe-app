using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeAPI.Controllers;
using RecipeAPI.FirestoreModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RecipeAPI.Tests
{
    [Trait("Category", "Unit")]
    public class ValuesControllerTests
    {
        private readonly Recipe TestingRecipe = new Recipe
        {
            Name = "Test Recipe",
            UserId = "123",
            Id = "recipe-456",
            CookTimeMins = 11,
            PrepTimeMins = 22,
            Servings = 99
        };

        [Fact]
        public void Get()
        {
            var valuesController = new ValuesApiController(null, null);
            var result = valuesController.Get();
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Get_UserId()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            logger.SetupAllProperties();

            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.GetAllRecipesForUser(It.IsAny<string>()))
                .ReturnsAsync(new List<Recipe> { TestingRecipe });

            var controller = new ValuesApiController(service.Object, logger.Object);
            var result = await controller.Get("123");

            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal(TestingRecipe.Id, resultList[0].RecipeId);
            Assert.Equal(TestingRecipe.UserId, resultList[0].UserId);
            Assert.Equal(TestingRecipe.Name, resultList[0].Name);
            Assert.Equal(TestingRecipe.Servings, resultList[0].Servings);
            Assert.Equal(TestingRecipe.PrepTimeMins, resultList[0].PrepTimeMins);

            service.Verify(s => s.GetAllRecipesForUser(It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Put_Simple(bool ok)
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            logger.SetupAllProperties();

            var recipeModel = new RecipeApp.Core.ExternalModels.RecipeModel
            {
                CookTimeMins = 60,
                Name = "Unit Test Recipe",
                RecipeId = "recipe-11111",
                UserId = "userId"
            };

            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.SaveRecipe(It.IsAny<Recipe>())).ReturnsAsync(ok);

            var controller = new ValuesApiController(service.Object, logger.Object);
            var result = await controller.Put(recipeModel.UserId, recipeModel.RecipeId, recipeModel);

            if (ok)
                Assert.IsType<AcceptedResult>(result);
            else
                Assert.IsType<BadRequestResult>(result);
        }
    }
}
