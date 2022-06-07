using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
        public override string LocalBaseUrl => "http://localhost:8080";

        public override string QaBaseUrl => "https://recipe-dataapi-qa.azurewebsites.net";

        public override string ProdUrl => "https://recipeapi.azurewebsites.net";

        private const string DefaultKey = "7ecb61f9642247a7b536f68d8cdf1cee";
        private const string ProdJwtIssuer = "https://recipe-ui.azurewebsites.net";
        private const string QaJwtIssuer = "https://recipe-ui-qa.azurewebsites.net";

        private readonly string ApiURL;
        private readonly string JwtKey;
        private readonly string TestEnvironment;

        private const string TestUserId = "abcde";
        private const string QAEnvironment = "QA";

        public RecipeAPITests()
        {
            this.TestEnvironment = Environment.GetEnvironmentVariable("RecipeEnv") ?? "local"; // To override it, either specify "local", "staging", or "prod"
            this.ApiURL = GetTestUrl(this.TestEnvironment);

            var key = Environment.GetEnvironmentVariable("RECIPE-INTERNAL-AUTH");
            if (!String.IsNullOrEmpty(key))
            {
                this.JwtKey = key;
            }
            else
            {
                Console.WriteLine("Couldn't find a key from the environment variable so using default.");
                this.JwtKey = DefaultKey;
            }

            Console.WriteLine($"Test environment is {this.TestEnvironment} hitting {this.ApiURL} and key {this.JwtKey.Substring(0, 4)}");
        }

        private string GenerateJWT()
        {
            string jwt;
            if (this.TestEnvironment == "QA" || this.TestEnvironment == "staging")
                jwt = Utilities.GenerateJWTToken(TestUserId, QaJwtIssuer, JwtKey);
            else if (this.TestEnvironment == "prod")
                jwt = Utilities.GenerateJWTToken(TestUserId, ProdJwtIssuer, JwtKey);
            else
                jwt = Utilities.GenerateJWTToken(TestUserId, LocalBaseUrl, JwtKey);

            return jwt;
        }

        [Fact]
        public async Task TestSave_and_Delete()
        {
            const int testRecipeId = 4567;
            var token = GenerateJWT();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var testRecipe = new RecipeModel()
                {
                    Name = "TESTINGTHIS",
                    RecipeId = testRecipeId,
                    UserId = TestUserId
                };

                var content = new StringContent(JsonSerializer.Serialize(testRecipe), Encoding.UTF8, "application/json");
                var result = await client.PutAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}", content);
                Assert.True(result.IsSuccessStatusCode, $"Received HTTP Status Code of {result.StatusCode}");

                var delete = await client.DeleteAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}");
                Assert.True(delete.IsSuccessStatusCode, $"Received HTTP Status Code of {result.StatusCode}");
            }
            finally
            {
                var delete1 = await client.DeleteAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}");
            }
        }
    }
}