using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeApp.Core.ExternalModels;
using System.Linq;
using System.Threading.Tasks;
using Website.Authorization;
using Website.Data;

namespace Website.Pages.Recipes
{
    #region snippet
    public class EditModel : DI_BasePageModel
    {
        public EditModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        [BindProperty]
        public RecipeModel Recipe { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Recipe = await Context.Recipe.FirstOrDefaultAsync(
                                                 m => m.RecipeId == id);

            if (Recipe == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                      User, Recipe,
                                                      RecipeOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Fetch Recipe from DB to get OwnerID.
            var recipe = await Context
                .Recipe.AsNoTracking()
                .FirstOrDefaultAsync(m => m.RecipeId == id);

            if (recipe == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, recipe,
                                                     RecipeOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            Recipe.UserId = recipe.UserId;

            Context.Attach(Recipe).State = EntityState.Modified;

            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private bool RecipeExists(int id)
        {
            return Context.Recipe.Any(e => e.RecipeId == id);
        }
    }
    #endregion
}
