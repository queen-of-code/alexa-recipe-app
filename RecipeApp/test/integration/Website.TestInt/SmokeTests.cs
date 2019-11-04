using System.Net.Http;
using System.Threading.Tasks;

using Integration;

using Xunit;

namespace Website.TestInt
{
    public class SmokeTests : IntegrationTestBase
    {
        private readonly string _baseUrl;
        private readonly HttpClient _client = new HttpClient();

        public override string LocalBaseUrl => "http://localhost:4000";

        public override string QaBaseUrl => "https://recipe-ui-qa.azurewebsites.net";

        public override string ProdUrl => "https://recipe-ui.azurewebsites.net";

        public SmokeTests()
        {
            this._baseUrl = GetTestUrl(); // To override it, either specify "local", "staging", or "prod"
        }

        [Fact]
        public async Task Get_Index()
        {
            var result = await _client.GetAsync($"{_baseUrl}/");
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
            var content = await result.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.True(content.Length > 10);
        }

        [Fact]
        public async Task Get_Contact()
        {
            var result = await _client.GetAsync($"{_baseUrl}/Contact");
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
            var content = await result.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.True(content.Length > 10);
        }


        [Fact]
        public async Task Get_About()
        {
            var result = await _client.GetAsync($"{_baseUrl}/About");
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
            var content = await result.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.True(content.Length > 10);
        }

        [Fact]
        public async Task Get_Login()
        {
            var result = await _client.GetAsync($"{_baseUrl}/Identity/Account/Login");
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
            var content = await result.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.True(content.Length > 10);
        }
    }
}
