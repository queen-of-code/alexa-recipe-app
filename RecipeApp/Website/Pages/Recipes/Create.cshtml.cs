using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Core.ExternalModels;
using System;
using System.Threading.Tasks;
using Website.Authorization;
using Website.Data;

namespace Website.Pages.Recipes
{
    #region snippetCtor
    public class CreateModel : DI_BasePageModel
    {
        public CreateModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }
        #endregion

        public IActionResult OnGet()
        {
            Recipe = new RecipeModel
            {
                Name = "Chicken Tikka",
                CookTimeMins = 30,
                LastUpdateTime = DateTime.UtcNow,
                PrepTimeMins = 10,
                Servings = 5
            };

            Recipe.Ingredients.AddRange(new string[] { "2lbs Chicken thighs", "2 whole tomatoes", "1 cup cream" });
            Recipe.Steps.AddRange(new string[] { "1. Cut up the chicken", "2. Cook the chicken", "3. Profit!" });
            return Page();
        }

        [BindProperty]
        public RecipeModel Recipe { get; set; }

        #region snippet_Create
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Recipe.UserId = UserManager.GetUserId(User);

            // requires using ContactManager.Authorization;
            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                        User, Recipe,
                                                        RecipeOperations.Create);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            Context.Recipe.Add(Recipe);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
        #endregion
    }
}