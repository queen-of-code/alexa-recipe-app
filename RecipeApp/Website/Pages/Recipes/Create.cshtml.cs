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
    public class CreateModel : DI_BasePageModel
    {
        private static readonly Random rand = new Random();

        private readonly RecipeService RecipeService;

        public CreateModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager,
            RecipeService recipeService)
            : base(context, authorizationService, userManager)
        {
            this.RecipeService = recipeService;
        }

        public IActionResult OnGet()
        {
            var userId = UserManager.GetUserId(this.User);
            if (userId == null)
                return new UnauthorizedResult();

            Recipe = new RecipeModel
            {
                Name = "Chicken Tikka",
                UserId = userId,
                RecipeId = rand.Next(),
                CookTimeMins = 45,
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
            if (Recipe.RecipeId == default(long))
            {
                Recipe.RecipeId = rand.Next();
            }

            // requires using ContactManager.Authorization;
            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                        User, Recipe,
                                                        RecipeOperations.Create);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            var result = await this.RecipeService.SaveRecipe(Recipe);
            if (!result)
            {
                return Page(); // Maybe return some error here instead?
            }

            return RedirectToPage("./Index");
        }
        #endregion
    }
}