using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RecipeApp.Core.ExternalModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Website
{
    public class RecipeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _JwtKey;
        private readonly string _JwtIssuer;

        public RecipeService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _JwtKey = config["RecipeConnectionKey"];
            _JwtIssuer = config["JwtIssuer"];
        }

        public async Task<bool> CreateRecipe(RecipeModel recipe)
        {
            var client = _httpClientFactory.CreateClient("RecipeAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetJwtAuth(recipe.UserId));

            var result = await client.PostAsJsonAsync($"/api/values/{recipe.UserId}", recipe);
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> SaveRecipe(RecipeModel recipe)
        {
            var client = _httpClientFactory.CreateClient("RecipeAPI");
            string jwt = GetJwtAuth(recipe.UserId);
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
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetJwtAuth(recipe.UserId));

            var result = await client.DeleteAsync($"/api/values/{recipe.UserId}/{recipe.RecipeId}");

            return result.IsSuccessStatusCode;
        }

        public async Task<RecipeModel> GetRecipe(string userId, string recipeId)
        {
            var client = _httpClientFactory.CreateClient("RecipeAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetJwtAuth(userId));

            var result = await client.GetAsync($"/api/values/{userId}/{recipeId}");
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
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetJwtAuth(userId));

            var result = await client.GetAsync($"/api/values/{userId}");
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

        private string GetJwtAuth(string userId)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken
            (
                issuer: _JwtIssuer,
                audience: _JwtIssuer,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(60),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtKey)),
                        SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
