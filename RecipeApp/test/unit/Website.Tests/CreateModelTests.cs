using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RecipeApp.Core.ExternalModels;
using Website.Data;
using Website.Pages.Recipes;
using Xunit;

namespace Website.Tests
{
    public class CreateModelTests
    {
        [Fact]
        public async Task OnPostAsync_ValidModel_Authorized_SavesRecipeAndRedirects()
        {
            // Arrange
            ApplicationDbContext mockDb = null;
            var mockAuth = new Mock<IAuthorizationService>();
            var mockUserMgr = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            var mockRecipeService = new Mock<IRecipeService>();

            // Set up user ID
            var testUserId = "user-123";
            mockUserMgr.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(testUserId);

            // Set up authorization
            mockAuth.Setup(a => a.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IAuthorizationRequirement[]>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Set up recipe service
            mockRecipeService.Setup(s => s.SaveRecipe(It.IsAny<RecipeModel>())).ReturnsAsync(true);

            var pageModel = new CreateModel(mockDb, mockAuth.Object, mockUserMgr.Object, mockRecipeService.Object)
            {
                Recipe = new RecipeModel { Name = "Test", PrepTimeMins = 1, CookTimeMins = 1, Servings = 1 }
            };

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var redirect = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./Index", redirect.PageName);
            mockRecipeService.Verify(s => s.SaveRecipe(It.IsAny<RecipeModel>()), Times.Once);
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

            mockAuth.Setup(a => a.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IAuthorizationRequirement[]>()))
                .ReturnsAsync(AuthorizationResult.Success());

            RecipeModel savedRecipe = null;
            mockRecipeService.Setup(s => s.SaveRecipe(It.IsAny<RecipeModel>()))
                .Callback<RecipeModel>(r => savedRecipe = r)
                .ReturnsAsync(true);

            var pageModel = new CreateModel(mockDb, mockAuth.Object, mockUserMgr.Object, mockRecipeService.Object)
            {
                Recipe = new RecipeModel { Name = "Test Recipe", PrepTimeMins = 10, CookTimeMins = 20, Servings = 4 },
                StepsText = "Preheat oven to 350°F\nMix all ingredients\nBake for 20 minutes\nLet cool"
            };

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var redirect = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./Index", redirect.PageName);
            
            Assert.NotNull(savedRecipe);
            Assert.Equal(4, savedRecipe.Steps.Count);
            Assert.Equal("Preheat oven to 350°F", savedRecipe.Steps[0]);
            Assert.Equal("Mix all ingredients", savedRecipe.Steps[1]);
            Assert.Equal("Bake for 20 minutes", savedRecipe.Steps[2]);
            Assert.Equal("Let cool", savedRecipe.Steps[3]);
        }

        [Fact]
        public async Task OnPostAsync_EmptyStepsText_CreatesEmptyStepsList()
        {
            // Arrange
            ApplicationDbContext mockDb = null;
            var mockAuth = new Mock<IAuthorizationService>();
            var mockUserMgr = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            var mockRecipeService = new Mock<IRecipeService>();

            var testUserId = "user-123";
            mockUserMgr.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(testUserId);

            mockAuth.Setup(a => a.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IAuthorizationRequirement[]>()))
                .ReturnsAsync(AuthorizationResult.Success());

            RecipeModel savedRecipe = null;
            mockRecipeService.Setup(s => s.SaveRecipe(It.IsAny<RecipeModel>()))
                .Callback<RecipeModel>(r => savedRecipe = r)
                .ReturnsAsync(true);

            var pageModel = new CreateModel(mockDb, mockAuth.Object, mockUserMgr.Object, mockRecipeService.Object)
            {
                Recipe = new RecipeModel { Name = "Test Recipe", PrepTimeMins = 10, CookTimeMins = 20, Servings = 4 },
                StepsText = ""
            };

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var redirect = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./Index", redirect.PageName);
            
            Assert.NotNull(savedRecipe);
            Assert.Empty(savedRecipe.Steps);
        }

        [Fact]
        public async Task OnPostAsync_StepsTextWithEmptyLines_FiltersOutEmptyLines()
        {
            // Arrange
            ApplicationDbContext mockDb = null;
            var mockAuth = new Mock<IAuthorizationService>();
            var mockUserMgr = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            var mockRecipeService = new Mock<IRecipeService>();

            var testUserId = "user-123";
            mockUserMgr.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(testUserId);

            mockAuth.Setup(a => a.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IAuthorizationRequirement[]>()))
                .ReturnsAsync(AuthorizationResult.Success());

            RecipeModel savedRecipe = null;
            mockRecipeService.Setup(s => s.SaveRecipe(It.IsAny<RecipeModel>()))
                .Callback<RecipeModel>(r => savedRecipe = r)
                .ReturnsAsync(true);

            var pageModel = new CreateModel(mockDb, mockAuth.Object, mockUserMgr.Object, mockRecipeService.Object)
            {
                Recipe = new RecipeModel { Name = "Test Recipe", PrepTimeMins = 10, CookTimeMins = 20, Servings = 4 },
                StepsText = "Step 1\n\n\nStep 2\n   \nStep 3\n"
            };

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var redirect = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./Index", redirect.PageName);
            
            Assert.NotNull(savedRecipe);
            Assert.Equal(3, savedRecipe.Steps.Count);
            Assert.Equal("Step 1", savedRecipe.Steps[0]);
            Assert.Equal("Step 2", savedRecipe.Steps[1]);
            Assert.Equal("Step 3", savedRecipe.Steps[2]);
        }
    }
} 