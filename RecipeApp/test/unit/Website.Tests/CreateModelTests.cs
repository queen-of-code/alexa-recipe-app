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
            var mockDb = new Mock<ApplicationDbContext>();
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

            var pageModel = new CreateModel(mockDb.Object, mockAuth.Object, mockUserMgr.Object, mockRecipeService.Object)
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
    }
} 