using System;
using System.Net.Http;
using System.Threading.Tasks;
using Integration;
using RecipeApp.Core;
using RecipeApp.Core.ExternalModels;
using Xunit;

namespace RecipeAPI.TestInt
{
    [Trait("Category", "Integration")]
    public class RecipeAPITests : IntegrationTestBase
    {
        public override string LocalBaseUrl => "http://localhost:4080";

        public override string QaBaseUrl => "https://recipe-dataapi-qa.azurewebsites.net";

        public override string ProdUrl => "https://recipeapi.azurewebsites.net";

        private const string DefaultKey = "7ecb61f9642247a7b536f68d8cdf1cee";
        private const string ProdJwtIssuer = "https://recipe-ui.azurewebsites.net";
        private const string QaJwtIssuer = "https://recipe-ui-qa.azurewebsites.net";

        private readonly string ApiURL;
        private readonly string JwtKey;
        private readonly HttpClient client;

        public RecipeAPITests()
        {
            this.ApiURL = GetTestUrl(); // To override it, either specify "local", "staging", or "prod"

            var key = Environment.GetEnvironmentVariable("RECIPE-INTERNAL-AUTH");
            if (!String.IsNullOrEmpty(key))
            {
                this.JwtKey = key;
            }
            else
            {
                this.JwtKey = DefaultKey;
            }

            client = new HttpClient();
            var jwt = Utilities.GenerateJWTToken("5", ProdJwtIssuer, JwtKey);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        }

        [Fact]
        public async Task TestSave_and_Delete()
        {
            try
            {
                var testRecipe = new RecipeModel()
                {
                    Name = "TESTINGTHIS",
                    RecipeId = 1,
                    UserId = "5"
                };
                var result = await client.PutAsJsonAsync<RecipeModel>($"{ApiURL}/api/values/5/1", testRecipe);
                Assert.True(result.IsSuccessStatusCode, $"Received HTTP Status Code of {result.StatusCode}");

                var delete = await client.DeleteAsync($"{ApiURL}/api/values/5/1");
                Assert.True(delete.IsSuccessStatusCode, $"Received HTTP Status Code of {result.StatusCode}");
            }
            finally
            {
                var delete1 = await client.DeleteAsync($"{ApiURL}/api/values/5/1");
            }
        }
    }
}