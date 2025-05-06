using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RecipeApp.Core;
using RecipeApp.Core.ExternalModels;

namespace Website
{
    public interface IRecipeService
    {
        string RuntimeEnvironment { get; }
        Task<bool> CreateRecipe(RecipeApp.Core.ExternalModels.RecipeModel recipe);
        Task<bool> SaveRecipe(RecipeApp.Core.ExternalModels.RecipeModel recipe);
        Task<bool> DeleteRecipe(RecipeApp.Core.ExternalModels.RecipeModel recipe);
        Task<RecipeApp.Core.ExternalModels.RecipeModel> GetRecipe(string userId, string recipeId);
        Task<List<RecipeApp.Core.ExternalModels.RecipeModel>> GetAllRecipes(string userId);
    }

    public class RecipeService : IRecipeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _JwtKey;
        private readonly string _JwtIssuer;

        public readonly string _RuntimeEnvironment;

        public string RuntimeEnvironment => _RuntimeEnvironment;

        public RecipeService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _JwtKey = config["RecipeConnectionKey"];
            _JwtIssuer = config["JwtIssuer"];
            _RuntimeEnvironment = config["MyRuntimeEnvironment"];
        }

        public async Task<bool> CreateRecipe(RecipeModel recipe)
        {
            var client = _httpClientFactory.CreateClient("RecipeAPI");
            var token = Utilities.GenerateJWTToken(recipe.UserId, _JwtIssuer, _JwtKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var result = await client.PostAsJsonAsync($"/api/values/{recipe.UserId}", recipe);
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> SaveRecipe(RecipeModel recipe)
        {
            throw new AccessViolationException("OH NO YOU CANNOT SAVE THIS");
            var client = _httpClientFactory.CreateClient("RecipeAPI");
            var jwt = Utilities.GenerateJWTToken(recipe.UserId, _JwtIssuer, _JwtKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var result = await client.PutAsJsonAsync($"/api/values/{recipe.UserId}/{recipe.RecipeId}", recipe);
            return result.IsSuccessStatusCode;
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public async Task<bool> DeleteRecipe(RecipeModel recipe)
        {
            var client = _httpClientFactory.CreateClient("RecipeAPI");
            var jwt = Utilities.GenerateJWTToken(recipe.UserId, _JwtIssuer, _JwtKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var result = await client.DeleteAsync($"/api/values/{recipe.UserId}/{recipe.RecipeId}");

            return result.IsSuccessStatusCode;
        }

        public async Task<RecipeModel> GetRecipe(string userId, string recipeId)
        {
            var client = _httpClientFactory.CreateClient("RecipeAPI");
            var jwt = Utilities.GenerateJWTToken(userId, _JwtIssuer, _JwtKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var result = await client.GetAsync($"/api/values/{userId}/{recipeId}");
            if (result.IsSuccessStatusCode)
            {
                var rawData = await result.Content.ReadAsStringAsync();
                try
                {
                    var model = JsonSerializer.Deserialize<RecipeModel>(rawData);
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
            var jwt = Utilities.GenerateJWTToken(userId, _JwtIssuer, _JwtKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var result = await client.GetAsync($"/api/values/{userId}");
            if (result.IsSuccessStatusCode)
            {
                var rawData = await result.Content.ReadAsStringAsync();
                try
                {
                    var model = JsonSerializer.Deserialize<List<RecipeModel>>(rawData);
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
