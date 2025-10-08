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

        private string GetJwtIssuer()
        {
            if (this.TestEnvironment == "QA" || this.TestEnvironment == "staging")
                return QaJwtIssuer;
            else if (this.TestEnvironment == "prod")
                return ProdJwtIssuer;
            else
                return LocalBaseUrl;
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

        [Fact]
        public async Task SaveRecipe_WithSteps_PersistsAndRetrievesSteps()
        {
            var testRecipeId = new Random().Next(1, 1000000);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                Utilities.GenerateJWTToken(TestUserId, GetJwtIssuer(), JwtKey));

            try
            {
                // Arrange - Create a recipe with steps
                var testRecipe = new RecipeModel
                {
                    RecipeId = testRecipeId,
                    Name = "Integration Test Recipe with Steps",
                    PrepTimeMins = 15,
                    CookTimeMins = 30,
                    Servings = 4,
                    UserId = TestUserId
                };
                testRecipe.Steps.AddRange(new[] { 
                    "Preheat oven to 350°F", 
                    "Mix all dry ingredients in a bowl", 
                    "Add wet ingredients and stir",
                    "Bake for 30 minutes or until golden brown",
                    "Let cool before serving"
                });
                testRecipe.Ingredients.AddRange(new[] { "Flour", "Sugar", "Eggs" });

                // Act - Save the recipe via API
                var saveContent = new StringContent(JsonSerializer.Serialize(testRecipe), Encoding.UTF8, "application/json");
                var saveResult = await client.PutAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}", saveContent);
                Assert.True(saveResult.IsSuccessStatusCode, $"Save failed with HTTP Status Code {saveResult.StatusCode}");

                // Act - Retrieve the recipe via API
                var getResult = await client.GetAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}");
                Assert.True(getResult.IsSuccessStatusCode, $"Get failed with HTTP Status Code {getResult.StatusCode}");

                var responseJson = await getResult.Content.ReadAsStringAsync();
                var retrievedRecipe = JsonSerializer.Deserialize<RecipeModel>(responseJson, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                // Assert - Verify steps were persisted correctly
                Assert.NotNull(retrievedRecipe);
                Assert.Equal(testRecipe.Name, retrievedRecipe.Name);
                Assert.Equal(testRecipe.PrepTimeMins, retrievedRecipe.PrepTimeMins);
                Assert.Equal(testRecipe.CookTimeMins, retrievedRecipe.CookTimeMins);
                Assert.Equal(testRecipe.Servings, retrievedRecipe.Servings);
                
                Assert.NotNull(retrievedRecipe.Steps);
                Assert.Equal(5, retrievedRecipe.Steps.Count);
                Assert.Equal("Preheat oven to 350°F", retrievedRecipe.Steps[0]);
                Assert.Equal("Mix all dry ingredients in a bowl", retrievedRecipe.Steps[1]);
                Assert.Equal("Add wet ingredients and stir", retrievedRecipe.Steps[2]);
                Assert.Equal("Bake for 30 minutes or until golden brown", retrievedRecipe.Steps[3]);
                Assert.Equal("Let cool before serving", retrievedRecipe.Steps[4]);

                Assert.NotNull(retrievedRecipe.Ingredients);
                Assert.Equal(3, retrievedRecipe.Ingredients.Count);
            }
            finally
            {
                var delete = await client.DeleteAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}");
            }
        }

        [Fact]
        public async Task SaveRecipe_WithEmptySteps_PersistsEmptyStepsList()
        {
            var testRecipeId = new Random().Next(1, 1000000);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                Utilities.GenerateJWTToken(TestUserId, GetJwtIssuer(), JwtKey));

            try
            {
                // Arrange - Create a recipe without steps
                var testRecipe = new RecipeModel
                {
                    RecipeId = testRecipeId,
                    Name = "Recipe Without Steps",
                    PrepTimeMins = 5,
                    CookTimeMins = 10,
                    Servings = 1,
                    UserId = TestUserId
                };
                // Don't add any steps - leave list empty

                // Act - Save and retrieve
                var saveContent = new StringContent(JsonSerializer.Serialize(testRecipe), Encoding.UTF8, "application/json");
                var saveResult = await client.PutAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}", saveContent);
                Assert.True(saveResult.IsSuccessStatusCode, $"Save failed with HTTP Status Code {saveResult.StatusCode}");

                var getResult = await client.GetAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}");
                Assert.True(getResult.IsSuccessStatusCode, $"Get failed with HTTP Status Code {getResult.StatusCode}");

                var responseJson = await getResult.Content.ReadAsStringAsync();
                var retrievedRecipe = JsonSerializer.Deserialize<RecipeModel>(responseJson, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                // Assert - Verify empty steps list is preserved
                Assert.NotNull(retrievedRecipe);
                Assert.Equal(testRecipe.Name, retrievedRecipe.Name);
                Assert.NotNull(retrievedRecipe.Steps);
                Assert.Empty(retrievedRecipe.Steps);
            }
            finally
            {
                var delete = await client.DeleteAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}");
            }
        }

        [Fact]
        public async Task UpdateRecipe_ModifySteps_PersistsNewSteps()
        {
            var testRecipeId = new Random().Next(1, 1000000);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                Utilities.GenerateJWTToken(TestUserId, GetJwtIssuer(), JwtKey));

            try
            {
                // Arrange - Create initial recipe with steps
                var initialRecipe = new RecipeModel
                {
                    RecipeId = testRecipeId,
                    Name = "Recipe to Update",
                    PrepTimeMins = 10,
                    CookTimeMins = 20,
                    Servings = 2,
                    UserId = TestUserId
                };
                initialRecipe.Steps.AddRange(new[] { "Original Step 1", "Original Step 2" });

                // Act - Save initial recipe
                var initialContent = new StringContent(JsonSerializer.Serialize(initialRecipe), Encoding.UTF8, "application/json");
                var initialResult = await client.PutAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}", initialContent);
                Assert.True(initialResult.IsSuccessStatusCode);

                // Act - Update recipe with new steps
                var updatedRecipe = new RecipeModel
                {
                    RecipeId = testRecipeId,
                    Name = "Updated Recipe",
                    PrepTimeMins = 15,
                    CookTimeMins = 25,
                    Servings = 4,
                    UserId = TestUserId
                };
                updatedRecipe.Steps.AddRange(new[] { "New Step 1", "New Step 2", "New Step 3" });

                var updateContent = new StringContent(JsonSerializer.Serialize(updatedRecipe), Encoding.UTF8, "application/json");
                var updateResult = await client.PutAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}", updateContent);
                Assert.True(updateResult.IsSuccessStatusCode);

                // Act - Retrieve updated recipe
                var getResult = await client.GetAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}");
                Assert.True(getResult.IsSuccessStatusCode);

                var responseJson = await getResult.Content.ReadAsStringAsync();
                var retrievedRecipe = JsonSerializer.Deserialize<RecipeModel>(responseJson, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                // Assert - Verify updated steps
                Assert.NotNull(retrievedRecipe);
                Assert.Equal("Updated Recipe", retrievedRecipe.Name);
                Assert.Equal(15, retrievedRecipe.PrepTimeMins);
                Assert.Equal(25, retrievedRecipe.CookTimeMins);
                Assert.Equal(4, retrievedRecipe.Servings);
                
                Assert.NotNull(retrievedRecipe.Steps);
                Assert.Equal(3, retrievedRecipe.Steps.Count);
                Assert.Equal("New Step 1", retrievedRecipe.Steps[0]);
                Assert.Equal("New Step 2", retrievedRecipe.Steps[1]);
                Assert.Equal("New Step 3", retrievedRecipe.Steps[2]);
            }
            finally
            {
                var delete = await client.DeleteAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}");
            }
        }
    }
}