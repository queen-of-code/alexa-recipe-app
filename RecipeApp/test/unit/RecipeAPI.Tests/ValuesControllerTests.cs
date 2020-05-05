using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using RecipeAPI.Controllers;

using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using RecipeAPI.DynamoModels;
using System;
using System.Linq;

namespace RecipeAPI.Tests
{
    [Trait("Category", "Unit")]
    public class ValuesControllerTests
    {
        private Recipe TestingRecipe = new Recipe()
        {
            Name = "Test Recipe",
            LastUpdateTime = DateTime.UtcNow,
            UserId = "123",
            RecipeId = 456,
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

            var dynamo = new Mock<IDynamoRecipeService>();
            dynamo.Setup(s =>
                s.GetAllRecipesForUser(It.IsAny<string>()))
                .ReturnsAsync(new List<Recipe>() { TestingRecipe });

            var valuesController = new ValuesApiController(dynamo.Object, logger.Object);

            var result = await valuesController.Get("123");
            Assert.NotNull(result);

            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal(TestingRecipe.RecipeId, resultList[0].RecipeId);
            Assert.Equal(TestingRecipe.UserId, resultList[0].UserId);
            Assert.Equal(TestingRecipe.Name, resultList[0].Name);
            Assert.Equal(TestingRecipe.LastUpdateTime, resultList[0].LastUpdateTime);
            Assert.Equal(TestingRecipe.Servings, resultList[0].Servings);
            Assert.Equal(TestingRecipe.PrepTimeMins, resultList[0].PrepTimeMins);

            dynamo.Verify(s => s.GetAllRecipesForUser(It.IsAny<string>()), Times.Once);
        }


    }
}
