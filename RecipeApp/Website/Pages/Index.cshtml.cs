using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Website.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IRecipeService RecipeService;

        public string RuntimeEnvironment { get; }

        public IndexModel(IRecipeService recipeService) 
            : base()
        {
            this.RecipeService = recipeService;
            this.RuntimeEnvironment = recipeService.RuntimeEnvironment;
        }

        public void OnGet()
        {

        }
    }
}
