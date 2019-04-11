using System.Net.Http;
using System.Threading.Tasks;

namespace Website
{
    public class RecipeService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RecipeService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> SaveRecipe(string recipe)
        {
            var client = _httpClientFactory.CreateClient("RecipeAPI");
            var result = await client.GetStringAsync("/ping");
            return result == null;
        }

        public string GetRecipe(string recipeId)
        {
            return "SOME RECIPE";
        }

        public string[] GetAllRecipes()
        {
            return new[] { "RECIPEA", "RECIPEB" };
        }

    }
}
