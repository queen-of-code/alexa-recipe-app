using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Core.ExternalModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website.Data;

namespace Website.Pages.Recipes
{
    public class IndexModel : DI_BasePageModel
    {
        private readonly IRecipeService RecipeService;

        public IndexModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager,
            IRecipeService recipeService)
            : base(context, authorizationService, userManager)
        {
            this.RecipeService = recipeService;
            this.RuntimeEnvironment = RecipeService._RuntimeEnvironment;
        }

        public IList<RecipeModel> Recipe { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUserId = UserManager.GetUserId(User);
            if (currentUserId == null)
                return new UnauthorizedResult();

            var recipes = await RecipeService.GetAllRecipes(currentUserId);
            if (recipes == null)
            {
                Recipe = new List<RecipeModel>(0);
            }
            else
            {
                Recipe = new List<RecipeModel>(recipes);
            }

            return Page();
        }
    }
}