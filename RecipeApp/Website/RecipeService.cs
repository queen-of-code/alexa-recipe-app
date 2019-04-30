using Newtonsoft.Json;
using RecipeApp.Core.ExternalModels;
using System;
using System.Collections.Generic;
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

        public async Task<bool> SaveRecipe(RecipeModel recipe)
        {
            var client = _httpClientFactory.CreateClient("RecipeAPI");

            var raw = JsonConvert.SerializeObject(recipe);
            Console.WriteLine(raw);
            var result = await client.PostAsJsonAsync($"/api/values/{recipe.UserId}", recipe);
            return result.IsSuccessStatusCode;
        }

        /// <summary>
        /// Deletes a recipe.
        /// TODO This should actually delete and not just post again.
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public async Task<bool> DeleteRecipe(RecipeModel recipe)
        {
            var client = _httpClientFactory.CreateClient("RecipeAPI");

            var raw = JsonConvert.SerializeObject(recipe);
            Console.WriteLine(raw);
            var result = await client.PostAsJsonAsync($"/api/values/{recipe.UserId}", recipe);
            return result.IsSuccessStatusCode;
        }

        public async Task<RecipeModel> GetRecipe(string userId, string recipeId)
        {
            var client = _httpClientFactory.CreateClient("RecipeAPI");
            var result =  await client.GetAsync($"/api/values/{userId}/{recipeId}");
            if (result.IsSuccessStatusCode)
            {
                var rawData = await result.Content.ReadAsStringAsync();
                try
                {
                    var model = JsonConvert.DeserializeObject<RecipeModel>(rawData);
                    return model;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<List<RecipeModel>> GetAllRecipes(String userId)
        {
            var client = _httpClientFactory.CreateClient("RecipeAPI");
            Console.WriteLine(client.BaseAddress);
            var result = await client.GetAsync($"/api/values/{userId}");
            Console.WriteLine($"Request went to {result.RequestMessage.RequestUri}");
            if (result.IsSuccessStatusCode)
            {
                var rawData = await result.Content.ReadAsStringAsync();
                try
                {
                    var model = JsonConvert.DeserializeObject<List<RecipeModel>>(rawData);
                    return model;
                }
                catch (Exception)
                {
                    return new List<RecipeModel>(0);
                }
            }
            else
            {
                return null;
            }
        }
    }

}
