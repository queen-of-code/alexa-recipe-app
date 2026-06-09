using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeAPI.Controllers;
using RecipeAPI.FirestoreModels;
using RecipeApp.Core.ExternalModels;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Security.Claims;
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

        private static Recipe RecipeWithIngredients(string id, string name, params string[] ings)
        {
            var r = new Recipe
            {
                Name = name,
                UserId = "123",
                Id = id,
                CookTimeMins = 1,
                PrepTimeMins = 1,
                Servings = 1,
            };
            r.Ingredients.AddRange(ings);
            return r;
        }

        private static Recipe RecipeWithFavorite(string id, bool isFavorite, DateTime lastUpdated)
        {
            var r = RecipeWithIngredients(id, id, "salt");
            r.IsFavorite = isFavorite;
            r.LastUpdateTime = Timestamp.FromDateTime(DateTime.SpecifyKind(lastUpdated, DateTimeKind.Utc));
            return r;
        }

        private static void SetFirebaseUser(ValuesApiController controller, string uid)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, uid) };
            var identity = new ClaimsIdentity(claims, "Firebase");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) },
            };
        }

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
            SetFirebaseUser(controller, "123");
            var result = await controller.Get("123");

            var ok = Assert.IsType<OkObjectResult>(result);
            var resultList = Assert.IsAssignableFrom<IEnumerable<RecipeModel>>(ok.Value).ToList();
            Assert.Single(resultList);
            Assert.Equal(TestingRecipe.Id, resultList[0].RecipeId);
            Assert.Equal(TestingRecipe.UserId, resultList[0].UserId);
            Assert.Equal(TestingRecipe.Name, resultList[0].Name);
            Assert.Equal(TestingRecipe.Servings, resultList[0].Servings);
            Assert.Equal(TestingRecipe.PrepTimeMins, resultList[0].PrepTimeMins);

            service.Verify(s => s.GetAllRecipesForUser(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Get_OrdersFavoritesFirst()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var olderFavorite = RecipeWithFavorite("f-old", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var newerNonFavorite = RecipeWithFavorite("n-new", false, new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc));
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.GetAllRecipesForUser("123"))
                .ReturnsAsync(new List<Recipe> { newerNonFavorite, olderFavorite });

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.Get("123");

            var ok = Assert.IsType<OkObjectResult>(result);
            var ids = Assert.IsAssignableFrom<IEnumerable<RecipeModel>>(ok.Value).Select(r => r.RecipeId).ToList();
            Assert.Equal(new[] { "f-old", "n-new" }, ids);
        }

        [Fact]
        public async Task Get_FavoritesOnly_ReturnsOnlyFavorites()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var favorite = RecipeWithFavorite("f1", true, new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));
            var notFavorite = RecipeWithFavorite("n1", false, new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc));
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.GetAllRecipesForUser("123"))
                .ReturnsAsync(new List<Recipe> { favorite, notFavorite });

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.Get("123", favoritesOnly: true);

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<RecipeModel>>(ok.Value).ToList();
            Assert.Single(list);
            Assert.Equal("f1", list[0].RecipeId);
            Assert.True(list[0].IsFavorite);
        }

        [Fact]
        public async Task SetFavorite_ReturnsUpdatedRecipe()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var saved = RecipeWithFavorite("r1", true, DateTime.UtcNow);
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.SetFavorite("123", "r1", true)).ReturnsAsync(saved);

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.SetFavorite("123", "r1");

            var ok = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<RecipeModel>(ok.Value);
            Assert.True(model.IsFavorite);
        }

        [Fact]
        public async Task SetFavorite_NotFound_WhenRecipeMissing()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.SetFavorite("123", "missing", true)).ReturnsAsync((Recipe)null);

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.SetFavorite("123", "missing");
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ClearFavorite_ReturnsUpdatedRecipe()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var saved = RecipeWithFavorite("r1", false, DateTime.UtcNow);
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.SetFavorite("123", "r1", false)).ReturnsAsync(saved);

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.ClearFavorite("123", "r1");

            var ok = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<RecipeModel>(ok.Value);
            Assert.False(model.IsFavorite);
        }

        [Fact]
        public async Task FavoriteEndpoints_UserId_Mismatch_Forbid()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var service = new Mock<IFirestoreRecipeService>();
            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "999");

            Assert.IsType<ForbidResult>(await controller.SetFavorite("123", "r1"));
            Assert.IsType<ForbidResult>(await controller.ClearFavorite("123", "r1"));
            service.Verify(s => s.SetFavorite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task Search_OrdersFavoritesFirst()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var favorite = RecipeWithFavorite("f1", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            favorite.Ingredients.Clear();
            favorite.Ingredients.Add("tomato");
            var notFavorite = RecipeWithFavorite("n1", false, new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc));
            notFavorite.Ingredients.Clear();
            notFavorite.Ingredients.Add("tomato");
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.GetAllRecipesForUser("123"))
                .ReturnsAsync(new List<Recipe> { notFavorite, favorite });

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.Search("123", new RecipeSearchRequest
            {
                Ingredients = new List<string> { "tomato" },
                Combine = "All",
            });

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<RecipeModel>>(ok.Value).ToList();
            Assert.Equal(2, list.Count);
            Assert.Equal("f1", list[0].RecipeId);
            Assert.True(list[0].IsFavorite);
        }

        [Fact]
        public async Task Get_UserId_Mismatch_Forbid()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var service = new Mock<IFirestoreRecipeService>();
            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "999");
            var result = await controller.Get("123");
            Assert.IsType<ForbidResult>(result);
            service.Verify(s => s.GetAllRecipesForUser(It.IsAny<string>()), Times.Never);
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
            service.Setup(s => s.SaveRecipe(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => ok ? r : null);

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, recipeModel.UserId);
            var result = await controller.Put(recipeModel.UserId, recipeModel.RecipeId, recipeModel);

            if (ok)
                Assert.IsType<AcceptedResult>(result);
            else
                Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Search_Structured_All_Filters()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var r1 = RecipeWithIngredients("1", "A", "tomato", "salt");
            var r2 = RecipeWithIngredients("2", "B", "tomato", "cheddar");
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.GetAllRecipesForUser("123"))
                .ReturnsAsync(new List<Recipe> { r1, r2 });

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.Search("123", new RecipeSearchRequest
            {
                Ingredients = new List<string> { "tomato", "cheddar" },
                Combine = "All",
            });

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<RecipeModel>>(ok.Value);
            Assert.Single(list);
            Assert.Equal("2", list.First().RecipeId);
        }

        [Fact]
        public async Task Search_Query_Or_NaturalLanguage()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var r1 = RecipeWithIngredients("1", "A", "basil");
            var r2 = RecipeWithIngredients("2", "B", "oregano");
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.GetAllRecipesForUser("123"))
                .ReturnsAsync(new List<Recipe> { r1, r2 });

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.Search("123", new RecipeSearchRequest
            {
                Query = "basil or oregano",
            });

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<RecipeModel>>(ok.Value);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public async Task Search_Invalid_BadRequest()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var service = new Mock<IFirestoreRecipeService>();
            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.Search("123", new RecipeSearchRequest());
            Assert.IsType<BadRequestResult>(result);
            service.Verify(s => s.GetAllRecipesForUser(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Search_UserId_Mismatch_Forbid()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var service = new Mock<IFirestoreRecipeService>();
            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "999");
            var result = await controller.Search("123", new RecipeSearchRequest
            {
                Ingredients = new List<string> { "salt" },
                Combine = "All",
            });
            Assert.IsType<ForbidResult>(result);
            service.Verify(s => s.GetAllRecipesForUser(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Search_Structured_Wins_Over_Query()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var r1 = RecipeWithIngredients("1", "A", "tomato");
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.GetAllRecipesForUser("123"))
                .ReturnsAsync(new List<Recipe> { r1 });

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.Search("123", new RecipeSearchRequest
            {
                Ingredients = new List<string> { "tomato" },
                Combine = "All",
                Query = "basil or oregano",
            });

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<RecipeModel>>(ok.Value);
            Assert.Single(list);
        }

        [Fact]
        public async Task Post_Returns201_WithRecipeId()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var savedRecipe = new Recipe
            {
                Name = "New",
                UserId = "u1",
                Id = "generated-id-abc",
                CookTimeMins = 0,
                PrepTimeMins = 0,
                Servings = 1,
            };
            savedRecipe.Ingredients.Add("a");
            savedRecipe.Steps.Add("b");

            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.SaveRecipe(It.IsAny<Recipe>())).ReturnsAsync(savedRecipe);

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "u1");

            var body = new RecipeModel
            {
                Name = "New",
                UserId = "u1",
                PrepTimeMins = 0,
                CookTimeMins = 0,
                Servings = 1,
            };
            body.Ingredients.Add("a");
            body.Steps.Add("b");

            var result = await controller.Post("u1", body);

            var created = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, created.StatusCode);
            var model = Assert.IsType<RecipeModel>(created.Value);
            Assert.Equal("generated-id-abc", model.RecipeId);
            Assert.Equal("u1", model.UserId);
        }
    }
}
