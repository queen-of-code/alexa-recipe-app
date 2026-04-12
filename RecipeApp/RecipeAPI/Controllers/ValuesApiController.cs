using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using RecipeAPI.FirestoreModels;

using RecipeApp.Core.ExternalModels;

namespace RecipeAPI.Controllers
{
    [Authorize(AuthenticationSchemes = "Firebase")]
    [ApiController]
    [Route("api/values")]
    public class ValuesApiController : ControllerBase
    {
        private readonly ILogger Logger;
        private readonly IFirestoreRecipeService RecipeService;

        public ValuesApiController(IFirestoreRecipeService service,
                                   ILogger<ValuesApiController> logger)
        {
            this.RecipeService = service;
            this.Logger = logger;
        }

        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            return new BadRequestResult();
        }

        // GET api/values/{userId}
        [HttpGet("{userId}")]
        public async Task<IEnumerable<RecipeModel>> Get(string userId)
        {
            var recipes = await RecipeService.GetAllRecipesForUser(userId).ConfigureAwait(false);
            return recipes?.Select(s => s.GenerateExternalRecipe());
        }

        // GET api/values/{userId}/{recipeId}
        [HttpGet("{userId}/{recipeId}")]
        public async Task<RecipeModel> Get(string userId, string recipeId)
        {
            var recipe = await RecipeService.RetrieveRecipe(userId, recipeId).ConfigureAwait(false);
            return recipe?.GenerateExternalRecipe();
        }

        // POST api/values/{userId}
        [HttpPost("{userId}")]
        public async Task<IActionResult> Post(string userId, RecipeModel value)
        {
            if (value == null)
            {
                Logger.LogWarning("Failed to parse a recipe on post.");
                return new BadRequestResult();
            }

            if (value.UserId != userId)
            {
                Logger.LogWarning($"Recipe had userId of {value.UserId} and it was posted to {userId}");
                return new BadRequestResult();
            }

            try
            {
                var result = await RecipeService.SaveRecipe(new Recipe(value)).ConfigureAwait(false);
                return result ? new OkResult() : new BadRequestResult();
            }
#pragma warning disable CA1031
            catch (Exception)
#pragma warning restore CA1031
            {
                return new BadRequestResult();
            }
        }

        // PUT api/values/{userId}/{recipeId}
        [HttpPut("{userId}/{recipeId}")]
        public async Task<IActionResult> Put(string userId, string recipeId, RecipeModel value)
        {
            var converted = new Recipe(value);
            if (string.IsNullOrWhiteSpace(converted.Id)) converted.Id = recipeId;
            if (string.IsNullOrWhiteSpace(converted.UserId)) converted.UserId = userId;

            var result = await RecipeService.SaveRecipe(converted).ConfigureAwait(false);
            return result ? new AcceptedResult() : new BadRequestResult();
        }

        // DELETE api/values/{userId}/{recipeId}
        [HttpDelete("{userId}/{recipeId}")]
        public async Task<IActionResult> Delete(string userId, string recipeId)
        {
            var result = await RecipeService.DeleteRecipe(userId, recipeId).ConfigureAwait(false);
            return result ? new OkResult() : new BadRequestResult();
        }
    }
}
