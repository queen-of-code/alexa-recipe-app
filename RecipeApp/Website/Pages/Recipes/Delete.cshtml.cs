using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Core.ExternalModels;
using System.Threading.Tasks;
using Website.Authorization;
using Website.Data;

namespace Website.Pages.Recipes
{
    public class DeleteModel : DI_BasePageModel
    {
        private readonly IRecipeService RecipeService;

        public DeleteModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager,
            IRecipeService recipeService)
            : base(context, authorizationService, userManager)
        {
            this.RecipeService = recipeService;
        }

        [BindProperty]
        public RecipeModel Recipe { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = UserManager.GetUserId(User);
            if (userId == null)
                return new UnauthorizedResult();

            Recipe = await RecipeService.GetRecipe(userId, id.ToString());
         
            if (Recipe == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, Recipe,
                                                     RecipeOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var recipe = await RecipeService.GetRecipe(UserManager.GetUserId(User), id.ToString());

            if (recipe == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, recipe,
                                                     RecipeOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            var deleted = await RecipeService.DeleteRecipe(recipe);

            return RedirectToPage("./Index");
        }
    }
}
