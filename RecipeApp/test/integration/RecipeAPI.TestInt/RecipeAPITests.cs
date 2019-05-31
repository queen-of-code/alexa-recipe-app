using System;
using System.Net.Http;
using System.Threading.Tasks;
using RecipeApp.Core.ExternalModels;
using Xunit;

namespace RecipeAPI.TestInt
{
    public class RecipeAPITests
    {
        private const string DefaultBaseUrl = "http://localhost:8080";
        private readonly string ApiURL;
        private readonly HttpClient client;
        public RecipeAPITests()
        {
            var websiteUrl = Environment.GetEnvironmentVariable("ApiUrl");
            if (!string.IsNullOrWhiteSpace(websiteUrl))
            {
                this.ApiURL = websiteUrl;
            }
            else
            {
                this.ApiURL = DefaultBaseUrl;
            }

            client = new HttpClient();
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
                    RecipeId = 5,
                    UserId = "1"
                };
                var result = await client.PutAsJsonAsync<RecipeModel>($"{ApiURL}/api/values/5/1", testRecipe);
                Assert.True(result.IsSuccessStatusCode);
                var delete = await client.DeleteAsync($"{ApiURL}/api/values/5/1");
                Assert.True(delete.IsSuccessStatusCode);
            }
            finally
            {
                var delete1 = await client.DeleteAsync($"{ApiURL}/api/values/5/1");
            }
        }
    }
}