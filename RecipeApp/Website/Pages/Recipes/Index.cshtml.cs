using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Data;
using Website.Authorization;
using RecipeApp.Core.ExternalModels;

namespace Website.Pages.Recipes
{
    #region snippet
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public IList<RecipeModel> Recipe { get; set; }

        public async Task OnGetAsync()
        {
            var recipes = from c in Context.Recipe
                           select c;

            var isAuthorized = User.IsInRole(Constants.RecipeAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            // Only approved recipes are shown UNLESS you're authorized to see them
            // or you are the owner.
            if (!isAuthorized)
            {
                recipes = recipes.Where(c => c.UserId == currentUserId);
            }

            Recipe = await recipes.ToListAsync();
        }
    }
    #endregion
}