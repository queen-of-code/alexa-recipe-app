using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeApp.Core.ExternalModels;
using System.Threading.Tasks;
using Website.Authorization;
using Website.Data;

namespace Website.Pages.Recipes
{
    #region snippet
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public RecipeModel Recipe { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Recipe = await Context.Recipe.FirstOrDefaultAsync(m => m.RecipeId == id);

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
            var contact = await Context.Recipe.FirstOrDefaultAsync(m => m.RecipeId == id);
            if (contact == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, contact, RecipeOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            Context.Recipe.Update(contact);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
    #endregion
}
