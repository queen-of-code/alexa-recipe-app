using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeAPI.Controllers;
using RecipeAPI.FirestoreModels;
using RecipeApp.Core.ExternalModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public async Task Put_WithoutIsFavorite_PreservesStoredFavorite()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var existing = RecipeAt("recipe-11111", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Recipe saved = null;
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.RetrieveRecipe("userId", "recipe-11111")).ReturnsAsync(existing);
            service.Setup(s => s.SaveRecipe(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => saved = r)
                .ReturnsAsync((Recipe r) => r);

            var incoming = new RecipeModel
            {
                CookTimeMins = 60,
                Name = "Renamed",
                RecipeId = "recipe-11111",
                UserId = "userId",
            };

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "userId");
            var result = await controller.Put("userId", "recipe-11111", incoming);

            Assert.IsType<AcceptedResult>(result);
            Assert.NotNull(saved);
            Assert.True(saved.IsFavorite);
            Assert.Equal("Renamed", saved.Name);
        }

        [Fact]
        public async Task Put_ExplicitIsFavoriteFalse_ClearsStoredFavorite()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            Recipe saved = null;
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.SaveRecipe(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => saved = r)
                .ReturnsAsync((Recipe r) => r);

            var incoming = new RecipeModel
            {
                CookTimeMins = 60,
                Name = "Renamed",
                RecipeId = "recipe-11111",
                UserId = "userId",
                IsFavorite = false,
            };

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "userId");
            var result = await controller.Put("userId", "recipe-11111", incoming);

            Assert.IsType<AcceptedResult>(result);
            Assert.NotNull(saved);
            Assert.False(saved.IsFavorite);
            service.Verify(s => s.RetrieveRecipe(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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

        private static Recipe RecipeAt(string id, bool isFavorite, DateTime updatedUtc, params string[] ings)
        {
            var recipe = RecipeWithIngredients(id, id, ings);
            recipe.IsFavorite = isFavorite;
            recipe.LastUpdateTime = Timestamp.FromDateTime(DateTime.SpecifyKind(updatedUtc, DateTimeKind.Utc));
            return recipe;
        }

        [Fact]
        public async Task Get_OrdersFavoritesFirstThenLastUpdatedDesc()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var olderFavorite = RecipeAt("fav-old", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var newerPlain = RecipeAt("plain-new", false, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.GetAllRecipesForUser("123"))
                .ReturnsAsync(new List<Recipe> { newerPlain, olderFavorite });

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.Get("123");

            var ok = Assert.IsType<OkObjectResult>(result);
            var ids = Assert.IsAssignableFrom<IEnumerable<RecipeModel>>(ok.Value).Select(r => r.RecipeId).ToArray();
            Assert.Equal(new[] { "fav-old", "plain-new" }, ids);
        }

        [Fact]
        public async Task Get_FavoritesOnly_ReturnsOnlyFavorites()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var favorite = RecipeAt("fav", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var plain = RecipeAt("plain", false, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.GetAllRecipesForUser("123"))
                .ReturnsAsync(new List<Recipe> { plain, favorite });

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.Get("123", favoritesOnly: true);

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<RecipeModel>>(ok.Value).ToList();
            Assert.Equal(new[] { "fav" }, list.Select(r => r.RecipeId).ToArray());
            Assert.True(list[0].IsFavorite);
        }

        [Fact]
        public async Task Search_OrdersFavoritesFirstAndIncludesFlag()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var plain = RecipeAt("plain", false, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "tomato");
            var favorite = RecipeAt("fav", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), "tomato");
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.GetAllRecipesForUser("123"))
                .ReturnsAsync(new List<Recipe> { plain, favorite });

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.Search("123", new RecipeSearchRequest
            {
                Ingredients = new List<string> { "tomato" },
                Combine = "All",
            });

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<RecipeModel>>(ok.Value).ToList();
            Assert.Equal(new[] { "fav", "plain" }, list.Select(r => r.RecipeId).ToArray());
            Assert.True(list[0].IsFavorite);
            Assert.False(list[1].IsFavorite);
        }

        [Fact]
        public async Task PutFavorite_SetsFlagAndReturnsRecipe()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var saved = RecipeAt("recipe-456", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.SetFavorite("123", "recipe-456", true))
                .ReturnsAsync(new FavoriteUpdateResult(true, saved));

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.PutFavorite("123", "recipe-456", new FavoriteRequest { Favorite = true });

            var ok = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<RecipeModel>(ok.Value);
            Assert.True(model.IsFavorite);
            Assert.Equal("recipe-456", model.RecipeId);
        }

        [Fact]
        public async Task PutFavorite_MalformedBody_BadRequest()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var service = new Mock<IFirestoreRecipeService>();
            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");

            var result = await controller.PutFavorite("123", "recipe-456", new FavoriteRequest { Favorite = false });

            Assert.IsType<BadRequestResult>(result);
            service.Verify(s => s.SetFavorite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task PutFavorite_UserIdMismatch_Forbid()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var service = new Mock<IFirestoreRecipeService>();
            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "999");

            var result = await controller.PutFavorite("123", "recipe-456", new FavoriteRequest { Favorite = true });

            Assert.IsType<ForbidResult>(result);
            service.Verify(s => s.SetFavorite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task DeleteFavorite_ClearsFlag()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var saved = RecipeAt("recipe-456", false, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.SetFavorite("123", "recipe-456", false))
                .ReturnsAsync(new FavoriteUpdateResult(true, saved));

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.DeleteFavorite("123", "recipe-456");

            var ok = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<RecipeModel>(ok.Value);
            Assert.False(model.IsFavorite);
        }

        [Fact]
        public async Task DeleteFavorite_MissingRecipe_NotFound()
        {
            var logger = new Mock<ILogger<ValuesApiController>>();
            var service = new Mock<IFirestoreRecipeService>();
            service.Setup(s => s.SetFavorite("123", "missing", false))
                .ReturnsAsync(new FavoriteUpdateResult(false, null));

            var controller = new ValuesApiController(service.Object, logger.Object);
            SetFirebaseUser(controller, "123");
            var result = await controller.DeleteFavorite("123", "missing");

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
