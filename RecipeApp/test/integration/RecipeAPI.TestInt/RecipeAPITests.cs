using System;
using System.Net.Http;
using System.Threading.Tasks;
using RecipeApp.Core;
using RecipeApp.Core.ExternalModels;
using Xunit;

namespace RecipeAPI.TestInt
{
    public class RecipeAPITests
    {
        private const string DefaultBaseUrl = "http://localhost:8080";
        private const string ProdBaseUrl = "https://recipe-dataapi.azurewebsites.net";
        private const string DefaultKey = "7ecb61f9642247a7b536f68d8cdf1cee";
        private const string JwtIssuer = "https://recipe-ui.azurewebsites.net";

        private readonly string ApiURL;
        private readonly string JwtKey;
        private readonly HttpClient client;

        public RecipeAPITests()
        {
            var websiteUrl = Environment.GetEnvironmentVariable("APIURL");
            if (!string.IsNullOrWhiteSpace(websiteUrl))
            {
                this.ApiURL = websiteUrl;
            }
            else
            {
                this.ApiURL = DefaultBaseUrl;
            }
            this.ApiURL = "https://recipe-dataapi.azurewebsites.net";

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
            var jwt = Utilities.GenerateJWTToken("1", JwtIssuer, JwtKey);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        }

        [Fact]
        [Trait("Category","Integration")]
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