using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Integration;
using RecipeApp.Core.ExternalModels;
using Xunit;

namespace RecipeAPI.TestInt
{
    [Trait("Category", "Integration")]
    public class RecipeAPITests : IntegrationTestBase
    {
        public override string LocalBaseUrl => "http://localhost:8081";
        public override string QaBaseUrl => Environment.GetEnvironmentVariable("CLOUD_RUN_API_URL") ?? LocalBaseUrl;
        public override string ProdUrl => Environment.GetEnvironmentVariable("CLOUD_RUN_API_URL") ?? LocalBaseUrl;

        private const string TestUserId = "integration-test-user";

        private readonly string ApiURL;
        private readonly string TestEnvironment;

        public RecipeAPITests()
        {
            TestEnvironment = Environment.GetEnvironmentVariable("RecipeEnv") ?? "local";
            ApiURL = GetTestUrl(TestEnvironment);
            Console.WriteLine($"Test environment is {TestEnvironment} hitting {ApiURL}");
        }

        // Gets a Firebase ID token for the test user.
        // - Local: uses the Auth Emulator REST API (no real credentials needed).
        // - Production/QA: expects a pre-issued token in FIREBASE_TEST_USER_TOKEN env var.
        private async Task<string> GetTestTokenAsync()
        {
            if (!string.Equals(TestEnvironment, "local", StringComparison.OrdinalIgnoreCase))
            {
                var token = Environment.GetEnvironmentVariable("FIREBASE_TEST_USER_TOKEN");
                if (string.IsNullOrEmpty(token))
                    throw new InvalidOperationException("FIREBASE_TEST_USER_TOKEN env var must be set for non-local integration tests.");
                return token;
            }

            // Firebase Auth Emulator: sign in (or create) a test user and get an ID token.
            const string emulatorHost = "http://localhost:9099";
            const string email = "integration-test@example.com";
            const string password = "test-password-123";

            using var http = new HttpClient();

            // Try to sign in first.
            var signInPayload = JsonSerializer.Serialize(new
            {
                email,
                password,
                returnSecureToken = true
            });

            var signInRes = await http.PostAsync(
                $"{emulatorHost}/identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=fake-api-key",
                new StringContent(signInPayload, Encoding.UTF8, "application/json"));

            if (!signInRes.IsSuccessStatusCode)
            {
                // User doesn't exist yet — create them.
                var signUpPayload = JsonSerializer.Serialize(new
                {
                    email,
                    password,
                    returnSecureToken = true
                });
                var signUpRes = await http.PostAsync(
                    $"{emulatorHost}/identitytoolkit.googleapis.com/v1/accounts:signUp?key=fake-api-key",
                    new StringContent(signUpPayload, Encoding.UTF8, "application/json"));
                signUpRes.EnsureSuccessStatusCode();
                signInRes = signUpRes;
            }

            var json = await signInRes.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(json);
            return node!["idToken"]!.GetValue<string>();
        }

        [Fact]
        public async Task TestSave_and_Delete()
        {
            var token = await GetTestTokenAsync();
            var testRecipeId = "integration-test-" + Guid.NewGuid().ToString("N")[..8];

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var testRecipe = new RecipeModel
                {
                    Name = "TESTINGTHIS",
                    RecipeId = testRecipeId,
                    UserId = TestUserId
                };

                var content = new StringContent(JsonSerializer.Serialize(testRecipe), Encoding.UTF8, "application/json");
                var result = await client.PutAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}", content);
                Assert.True(result.IsSuccessStatusCode, $"Received HTTP Status Code of {result.StatusCode}");

                var delete = await client.DeleteAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}");
                Assert.True(delete.IsSuccessStatusCode, $"Delete received HTTP Status Code of {delete.StatusCode}");
            }
            finally
            {
                await client.DeleteAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}");
            }
        }

        [Fact]
        public async Task SaveRecipe_WithSteps_PersistsAndRetrievesSteps()
        {
            var token = await GetTestTokenAsync();
            var testRecipeId = "integration-steps-" + Guid.NewGuid().ToString("N")[..8];

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
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

                var saveContent = new StringContent(JsonSerializer.Serialize(testRecipe), Encoding.UTF8, "application/json");
                var saveResult = await client.PutAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}", saveContent);
                Assert.True(saveResult.IsSuccessStatusCode, $"Save failed with HTTP Status Code {saveResult.StatusCode}");

                var getResult = await client.GetAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}");
                Assert.True(getResult.IsSuccessStatusCode, $"Get failed with HTTP Status Code {getResult.StatusCode}");

                var responseJson = await getResult.Content.ReadAsStringAsync();
                var retrievedRecipe = JsonSerializer.Deserialize<RecipeModel>(responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Assert.NotNull(retrievedRecipe);
                Assert.Equal(testRecipe.Name, retrievedRecipe.Name);
                Assert.Equal(5, retrievedRecipe.Steps.Count);
                Assert.Equal("Preheat oven to 350°F", retrievedRecipe.Steps[0]);
                Assert.Equal(3, retrievedRecipe.Ingredients.Count);
            }
            finally
            {
                await client.DeleteAsync($"{ApiURL}/api/values/{TestUserId}/{testRecipeId}");
            }
        }
    }
}
