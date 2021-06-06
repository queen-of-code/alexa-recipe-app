using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Website.Pages
{
    public class IndexModel : PageModel
    {
        private readonly RecipeService RecipeService;

        public readonly string RuntimeEnvironment;

        public IndexModel(RecipeService recipeService) 
            : base()
        {
            this.RecipeService = recipeService;
            this.RuntimeEnvironment = RecipeService._RuntimeEnvironment;
        }

        public void OnGet()
        {

        }
    }
}
