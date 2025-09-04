using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using RecipeApp.Core.ExternalModels;
using Website.Data;
using Website.Pages.Recipes;
using Xunit;

namespace Website.Tests
{
    public class EditModelTests
    {
        [Fact]
        public async Task OnGetAsync_ConvertsStepsListToStepsText()
        {
            // Arrange
            ApplicationDbContext mockDb = null;
            var mockAuth = new Mock<IAuthorizationService>();
            var mockUserMgr = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            var mockRecipeService = new Mock<IRecipeService>();

            var testUserId = "user-123";
            mockUserMgr.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(testUserId);

            var testRecipe = new RecipeModel
            {
                UserId = testUserId,
                RecipeId = 456,
                Name = "Test Recipe",
                PrepTimeMins = 15,
                CookTimeMins = 30,
                Servings = 4
            };
            testRecipe.Steps.AddRange(new[] { "Preheat oven", "Mix ingredients", "Bake for 30 minutes" });

            mockRecipeService.Setup(s => s.GetRecipe(testUserId, "456")).ReturnsAsync(testRecipe);

            mockAuth.Setup(a => a.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IAuthorizationRequirement[]>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var pageModel = new EditModel(mockDb, mockAuth.Object, mockUserMgr.Object, mockRecipeService.Object);

            // Act
            var result = await pageModel.OnGetAsync(456);

            // Assert
            Assert.IsType<PageResult>(result);
            Assert.NotNull(pageModel.Recipe);
            Assert.Equal("Test Recipe", pageModel.Recipe.Name);
            
            Assert.NotNull(pageModel.StepsText);
            Assert.Equal("Preheat oven\nMix ingredients\nBake for 30 minutes", pageModel.StepsText);
        }

        [Fact]
        public async Task OnGetAsync_EmptySteps_CreatesEmptyStepsText()
        {
            // Arrange
            ApplicationDbContext mockDb = null;
            var mockAuth = new Mock<IAuthorizationService>();
            var mockUserMgr = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            var mockRecipeService = new Mock<IRecipeService>();

            var testUserId = "user-123";
            mockUserMgr.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(testUserId);

            var testRecipe = new RecipeModel
            {
                UserId = testUserId,
                RecipeId = 789,
                Name = "Recipe Without Steps",
                PrepTimeMins = 10,
                CookTimeMins = 20,
                Servings = 2
            };
            // Steps list is empty by default

            mockRecipeService.Setup(s => s.GetRecipe(testUserId, "789")).ReturnsAsync(testRecipe);

            mockAuth.Setup(a => a.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IAuthorizationRequirement[]>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var pageModel = new EditModel(mockDb, mockAuth.Object, mockUserMgr.Object, mockRecipeService.Object);

            // Act
            var result = await pageModel.OnGetAsync(789);

            // Assert
            Assert.IsType<PageResult>(result);
            Assert.NotNull(pageModel.Recipe);
            Assert.Equal("Recipe Without Steps", pageModel.Recipe.Name);
            
            Assert.True(string.IsNullOrEmpty(pageModel.StepsText));
        }

        [Fact]
        public async Task OnPostAsync_ConvertsStepsTextToStepsList()
        {
            // Arrange
            ApplicationDbContext mockDb = null;
            var mockAuth = new Mock<IAuthorizationService>();
            var mockUserMgr = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            var mockRecipeService = new Mock<IRecipeService>();

            var testUserId = "user-123";
            mockUserMgr.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(testUserId);

            var existingRecipe = new RecipeModel
            {
                UserId = testUserId,
                RecipeId = 123,
                Name = "Original Recipe",
                PrepTimeMins = 5,
                CookTimeMins = 15,
                Servings = 2
            };

            mockRecipeService.Setup(s => s.GetRecipe(testUserId, "123")).ReturnsAsync(existingRecipe);

            mockAuth.Setup(a => a.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IAuthorizationRequirement[]>()))
                .ReturnsAsync(AuthorizationResult.Success());

            RecipeModel savedRecipe = null;
            mockRecipeService.Setup(s => s.SaveRecipe(It.IsAny<RecipeModel>()))
                .Callback<RecipeModel>(r => savedRecipe = r)
                .ReturnsAsync(true);

            var pageModel = new EditModel(mockDb, mockAuth.Object, mockUserMgr.Object, mockRecipeService.Object)
            {
                Recipe = new RecipeModel { Name = "Updated Recipe", PrepTimeMins = 10, CookTimeMins = 25, Servings = 4 },
                StepsText = "Step 1: Prepare\nStep 2: Cook\nStep 3: Serve"
            };

            // Act
            var result = await pageModel.OnPostAsync(123);

            // Assert
            var redirect = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./Index", redirect.PageName);
            
            Assert.NotNull(savedRecipe);
            Assert.Equal(3, savedRecipe.Steps.Count);
            Assert.Equal("Step 1: Prepare", savedRecipe.Steps[0]);
            Assert.Equal("Step 2: Cook", savedRecipe.Steps[1]);
            Assert.Equal("Step 3: Serve", savedRecipe.Steps[2]);
        }

        [Fact]
        public async Task OnPostAsync_EmptyStepsText_ClearsStepsList()
        {
            // Arrange
            ApplicationDbContext mockDb = null;
            var mockAuth = new Mock<IAuthorizationService>();
            var mockUserMgr = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            var mockRecipeService = new Mock<IRecipeService>();

            var testUserId = "user-123";
            mockUserMgr.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(testUserId);

            var existingRecipe = new RecipeModel
            {
                UserId = testUserId,
                RecipeId = 123,
                Name = "Original Recipe",
                PrepTimeMins = 5,
                CookTimeMins = 15,
                Servings = 2
            };

            mockRecipeService.Setup(s => s.GetRecipe(testUserId, "123")).ReturnsAsync(existingRecipe);

            mockAuth.Setup(a => a.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IAuthorizationRequirement[]>()))
                .ReturnsAsync(AuthorizationResult.Success());

            RecipeModel savedRecipe = null;
            mockRecipeService.Setup(s => s.SaveRecipe(It.IsAny<RecipeModel>()))
                .Callback<RecipeModel>(r => savedRecipe = r)
                .ReturnsAsync(true);

            var pageModel = new EditModel(mockDb, mockAuth.Object, mockUserMgr.Object, mockRecipeService.Object)
            {
                Recipe = new RecipeModel { Name = "Updated Recipe", PrepTimeMins = 10, CookTimeMins = 25, Servings = 4 },
                StepsText = ""
            };

            // Act
            var result = await pageModel.OnPostAsync(123);

            // Assert
            var redirect = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./Index", redirect.PageName);
            
            Assert.NotNull(savedRecipe);
            Assert.Empty(savedRecipe.Steps);
        }
    }
}
