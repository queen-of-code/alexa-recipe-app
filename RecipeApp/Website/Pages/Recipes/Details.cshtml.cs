using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Core.ExternalModels;
using System.Threading.Tasks;
using Website.Authorization;
using Website.Data;

namespace Website.Pages.Recipes
{
    public class DetailsModel : DI_BasePageModel
    {
        private readonly RecipeService RecipeService;

        public DetailsModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager,
            RecipeService recipeService)
            : base(context, authorizationService, userManager)
        {
            this.RecipeService = recipeService;
        }

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

            var isAuthorized = User.IsInRole(Constants.RecipeAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            if (!isAuthorized &&  currentUserId != Recipe.UserId) 
            {
                return new ChallengeResult();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var result = await RecipeService.SaveRecipe(Recipe);
            if (result)
            {
                return NotFound();
            }

            return RedirectToPage("./Index");
        }
    }
}
