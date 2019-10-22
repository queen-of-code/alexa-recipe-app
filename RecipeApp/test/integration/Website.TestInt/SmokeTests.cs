using System.Net.Http;
using System.Threading.Tasks;

using Integration;

using Xunit;

namespace Website.TestInt
{
    public class SmokeTests : IntegrationTestBase
    {
        private readonly string BaseUrl;
        private readonly HttpClient client = new HttpClient();

        public override string LocalBaseUrl => "http://localhost:4000";

        public override string QaBaseUrl => "https://recipe-ui-qa.azurewebsites.net";

        public override string ProdUrl => "https://recipe-ui.azurewebsites.net";

        public SmokeTests()
        {
            this.BaseUrl = GetTestUrl(); // To override it, either specify "local", "staging", or "prod"
        }

        [Fact]
        public async Task Get_Index()
        {
            var result = await client.GetAsync($"{BaseUrl}/");
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
            var content = await result.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.True(content.Length > 10);
        }

        [Fact]
        public async Task Get_Contact()
        {
            var result = await client.GetAsync($"{BaseUrl}/Contact");
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
            var content = await result.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.True(content.Length > 10);
        }


        [Fact]
        public async Task Get_About()
        {
            var result = await client.GetAsync($"{BaseUrl}/About");
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
            var content = await result.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.True(content.Length > 10);
        }

        [Fact]
        public async Task Get_Login()
        {
            var result = await client.GetAsync($"{BaseUrl}/Identity/Account/Login");
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
            var content = await result.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.True(content.Length > 10);
        }
    }
}
