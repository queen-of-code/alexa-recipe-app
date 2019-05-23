using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecipeAPI.DynamoModels;
using RecipeApp.Core.ExternalModels;

namespace RecipeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger Logger;
        private readonly DynamoRecipeService RecipeService;

        public ValuesController(DynamoRecipeService service,
                                ILogger<ValuesController> logger)
        {
            this.RecipeService = service;
            this.Logger = logger;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new BadRequestResult();
        }

        // GET api/values/5
        [HttpGet("{userId}")]
        public async Task<IEnumerable<RecipeModel>> Get(string userId)
        {
            var recipes = await RecipeService.GetAllRecipesForUser(userId);

            var converted = recipes?.Select(s => s.GenerateExternalRecipe());

            return converted;
        }
        
        // GET api/values/5/123
        [HttpGet("{userId}/{recipeId}")]
        public async Task<RecipeModel> Get(string userId, long recipeId)
        {
            var recipe = await RecipeService.RetrieveRecipe(userId, recipeId);

            var converted = recipe.GenerateExternalRecipe();

            return converted;
        }

        // POST api/values
        [HttpPost("{userId}")]
        public async Task<IActionResult> Post(string userId, [FromBody] RecipeModel value)
        {
            if (value == null)
            {
                Logger.LogWarning("Failed to parse a recipe on post.");
                return new BadRequestResult();
            }

            if (value.UserId != userId)
            {
                Logger.LogWarning($"Recipe had userid of {value.UserId} and it was posted to {userId}");
                return new BadRequestResult();
            }

            try
            {
                var result = await RecipeService.SaveRecipe(new Recipe(value));
                if (result)
                {
                    return new OkResult();
                }
                else
                {
                    Logger.LogWarning($"Failed to save the recipe into Dynamo for some reason.");
                    return new BadRequestResult();
                }
            }
            catch (Exception)
            {
                return new BadRequestResult();
            }

        }

        // PUT api/values/5
        [HttpPut("{userId}/{recipeId}")]
        public async Task<IActionResult> Put(string userId, string recipeId, [FromBody] RecipeModel value)
        {
            var converted = new Recipe(value);
            var result = await RecipeService.SaveRecipe(converted);

            if (result)
                return new AcceptedResult();

            return new BadRequestResult();
        }

        // DELETE api/values/5/1223
        [HttpDelete("{userId}/{recipeId}")]
        public async Task<IActionResult> Delete(string userId, long recipeId)
        {
            var result = await RecipeService.DeleteRecipe(userId, recipeId);

            if (result)
                return new OkResult();

            return new BadRequestResult();
        }
    }
}
