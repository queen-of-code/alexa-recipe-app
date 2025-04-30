using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using RecipeApp.Core.ExternalModels;
using Microsoft.Extensions.Configuration;
using RecipeApp.Core.Services;

namespace Website.Tests
{
    public class RecipeServiceTests
    {
        //[Trait("Category", "Unit")]
        //[Fact]
        public async Task TestCreateRecipe_Ok()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.SetupGet(s => s["RecipeConnectionKey"]).Returns("12345678901234567890123456784567");
            mockConfig.SetupGet(s => s["JwtIssuer"]).Returns("12345");

            var mockMessageHandler = new Mock<HttpMessageHandler>();
            mockMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                });

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s =>
                s.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(mockMessageHandler.Object));

            var recipeService = new RecipeService(httpClientFactory.Object, mockConfig.Object);

            var testRecipe = new RecipeModel { UserId = "123", RecipeId = 987 };

            var result = await recipeService.CreateRecipe(testRecipe);
            Assert.True(result);
        }
    }
}
