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
        // - Production/QA: signs in via Firebase REST API using FIREBASE_TEST_EMAIL,
        //   FIREBASE_TEST_PASSWORD, and FIREBASE_API_KEY env vars.
        private async Task<string> GetTestTokenAsync()
        {
            bool isLocal = string.Equals(TestEnvironment, "local", StringComparison.OrdinalIgnoreCase);
            string signInUrl, signUpUrl, apiKey, email, password;

            if (isLocal)
            {
                apiKey = "fake-api-key";
                email = "integration-test@example.com";
                password = "test-password-123";
                signInUrl = $"http://localhost:9099/identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";
                signUpUrl  = $"http://localhost:9099/identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}";
            }
            else
            {
                apiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY")
                    ?? throw new InvalidOperationException("FIREBASE_API_KEY env var must be set for non-local integration tests.");
                email = Environment.GetEnvironmentVariable("FIREBASE_TEST_EMAIL")
                    ?? throw new InvalidOperationException("FIREBASE_TEST_EMAIL env var must be set for non-local integration tests.");
                password = Environment.GetEnvironmentVariable("FIREBASE_TEST_PASSWORD")
                    ?? throw new InvalidOperationException("FIREBASE_TEST_PASSWORD env var must be set for non-local integration tests.");
                signInUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";
                signUpUrl  = signInUrl; // not used for production
            }

            using var http = new HttpClient();

            var signInPayload = JsonSerializer.Serialize(new { email, password, returnSecureToken = true });
            var signInRes = await http.PostAsync(signInUrl,
                new StringContent(signInPayload, Encoding.UTF8, "application/json"));

            if (!signInRes.IsSuccessStatusCode && isLocal)
            {
                // Emulator only: create the user if it doesn't exist yet.
                var signUpPayload = JsonSerializer.Serialize(new { email, password, returnSecureToken = true });
                var signUpRes = await http.PostAsync(signUpUrl,
                    new StringContent(signUpPayload, Encoding.UTF8, "application/json"));
                signUpRes.EnsureSuccessStatusCode();
                signInRes = signUpRes;
            }
            else
            {
                signInRes.EnsureSuccessStatusCode();
            }

            var json = await signInRes.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(json);
            return node!["idToken"]!.GetValue<string>();
        }

        /// <summary>
        /// Firebase UID from the ID token (<c>sub</c> claim). Must match route <c>userId</c> after EnsureRouteUserMatchesToken.
        /// </summary>
        private static string GetUidFromIdToken(string idToken)
        {
            var parts = idToken.Split('.');
            if (parts.Length != 3)
                throw new InvalidOperationException("Expected a Firebase ID token (JWT with 3 segments).");
            var payload = parts[1];
            var padded = payload.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            var node = JsonNode.Parse(json);
            return node!["sub"]!.GetValue<string>();
        }

        [Fact]
        public async Task TestSave_and_Delete()
        {
            var token = await GetTestTokenAsync();
            var userId = GetUidFromIdToken(token);
            var testRecipeId = "integration-test-" + Guid.NewGuid().ToString("N")[..8];

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var testRecipe = new RecipeModel
                {
                    Name = "TESTINGTHIS",
                    RecipeId = testRecipeId,
                    UserId = userId
                };

                var content = new StringContent(JsonSerializer.Serialize(testRecipe), Encoding.UTF8, "application/json");
                var result = await client.PutAsync($"{ApiURL}/api/values/{userId}/{testRecipeId}", content);
                Assert.True(result.IsSuccessStatusCode, $"Received HTTP Status Code of {result.StatusCode}");

                var delete = await client.DeleteAsync($"{ApiURL}/api/values/{userId}/{testRecipeId}");
                Assert.True(delete.IsSuccessStatusCode, $"Delete received HTTP Status Code of {delete.StatusCode}");
            }
            finally
            {
                await client.DeleteAsync($"{ApiURL}/api/values/{userId}/{testRecipeId}");
            }
        }

        [Fact]
        public async Task SaveRecipe_WithSteps_PersistsAndRetrievesSteps()
        {
            var token = await GetTestTokenAsync();
            var userId = GetUidFromIdToken(token);
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
                    UserId = userId
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
                var saveResult = await client.PutAsync($"{ApiURL}/api/values/{userId}/{testRecipeId}", saveContent);
                Assert.True(saveResult.IsSuccessStatusCode, $"Save failed with HTTP Status Code {saveResult.StatusCode}");

                var getResult = await client.GetAsync($"{ApiURL}/api/values/{userId}/{testRecipeId}");
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
                await client.DeleteAsync($"{ApiURL}/api/values/{userId}/{testRecipeId}");
            }
        }

        [Fact]
        public async Task Post_Create_ReturnsRecipeId_InBody()
        {
            var token = await GetTestTokenAsync();
            var userId = GetUidFromIdToken(token);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var testRecipe = new RecipeModel
            {
                Name = "POST create test " + Guid.NewGuid().ToString("N")[..8],
                UserId = userId,
                PrepTimeMins = 1,
                CookTimeMins = 2,
                Servings = 2,
            };
            testRecipe.Ingredients.Add("salt");
            testRecipe.Steps.Add("mix");

            var content = new StringContent(JsonSerializer.Serialize(testRecipe), Encoding.UTF8, "application/json");
            var postResult = await client.PostAsync($"{ApiURL}/api/values/{userId}", content);
            Assert.True(postResult.IsSuccessStatusCode, $"POST failed: {postResult.StatusCode}");
            Assert.Equal(System.Net.HttpStatusCode.Created, postResult.StatusCode);

            var body = await postResult.Content.ReadAsStringAsync();
            var created = JsonSerializer.Deserialize<RecipeModel>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(created);
            Assert.False(string.IsNullOrWhiteSpace(created.RecipeId));
            Assert.Equal(userId, created.UserId);

            await client.DeleteAsync($"{ApiURL}/api/values/{userId}/{created.RecipeId}");
        }

        [Fact]
        public async Task Put_WithCompletedImageUrl_RoundTrips()
        {
            var token = await GetTestTokenAsync();
            var userId = GetUidFromIdToken(token);
            var testRecipeId = "integration-img-" + Guid.NewGuid().ToString("N")[..8];
            var allowedUrl =
                $"https://firebasestorage.googleapis.com/v0/b/queen-of-code.appspot.com/o/users%2F{Uri.EscapeDataString(userId)}%2Frecipes%2F{testRecipeId}%2Fcompleted.jpg?alt=media";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var testRecipe = new RecipeModel
                {
                    RecipeId = testRecipeId,
                    Name = "With image",
                    UserId = userId,
                    PrepTimeMins = 1,
                    CookTimeMins = 1,
                    Servings = 1,
                    CompletedImageUrl = allowedUrl,
                };
                testRecipe.Ingredients.Add("a");
                testRecipe.Steps.Add("b");

                var saveContent = new StringContent(JsonSerializer.Serialize(testRecipe), Encoding.UTF8, "application/json");
                var saveResult = await client.PutAsync($"{ApiURL}/api/values/{userId}/{testRecipeId}", saveContent);
                Assert.True(saveResult.IsSuccessStatusCode, saveResult.StatusCode.ToString());

                var getResult = await client.GetAsync($"{ApiURL}/api/values/{userId}/{testRecipeId}");
                getResult.EnsureSuccessStatusCode();
                var json = await getResult.Content.ReadAsStringAsync();
                var got = JsonSerializer.Deserialize<RecipeModel>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Assert.Equal(allowedUrl, got.CompletedImageUrl);
            }
            finally
            {
                await client.DeleteAsync($"{ApiURL}/api/values/{userId}/{testRecipeId}");
            }
        }
    }
}
