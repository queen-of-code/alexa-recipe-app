
using RecipeApp.Core.ExternalModels;
using System.Net.Http;
using Xunit;

namespace RecipeAPI.TestInt
{
    public class RecipeAPITests
    {
        private string ApiURL;
        private HttpClient client;

        public RecipeAPITests()
        {
            ApiURL = "http://localhost:8080";
            client = new HttpClient();
        }


        [Fact]
        [Trait("Category","Integration")]
        public async void TestSave_and_Delete()
        {
            var testRecipe = new RecipeModel()
            {
                RecipeId = 1,
                UserId = "5",
                Name = "TESTINGTHIS"
            };
            var result = await client.PutAsJsonAsync<RecipeModel>($"{ApiURL}/api/values/5/1", testRecipe);

            Assert.True(result.IsSuccessStatusCode);
        }
    }
}
