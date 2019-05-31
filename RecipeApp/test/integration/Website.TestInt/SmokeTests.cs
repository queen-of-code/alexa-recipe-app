using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Website.TestInt
{
    public class SmokeTests
    {
        private const string DefaultBaseUrl = "http://localhost:5000";

        private readonly string BaseUrl;
        private readonly HttpClient client = new HttpClient();

        public SmokeTests()
        {
            var websiteUrl = Environment.GetEnvironmentVariable("WebsiteUrl");

            if (!string.IsNullOrWhiteSpace(websiteUrl))
            {
                this.BaseUrl = websiteUrl;
            }
            else
            {
                this.BaseUrl = DefaultBaseUrl;
            }
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
