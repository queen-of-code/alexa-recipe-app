using System.Collections.Generic;
using System.Linq;
using RecipeAPI.FirestoreModels;
using RecipeAPI.IngredientMatching;
using Xunit;

namespace RecipeAPI.Tests
{
    [Trait("Category", "Unit")]
    public class IngredientMatcherTests
    {
        private static Recipe R(string name, params string[] ingredients)
        {
            var r = new Recipe
            {
                Name = name,
                UserId = "u1",
                Id = name,
            };
            r.Ingredients.AddRange(ingredients);
            return r;
        }

        [Fact]
        public void Filter_All_RequiresEveryTerm()
        {
            var recipes = new[]
            {
                R("a", "2 cups diced Tomatoes", "salt"),
                R("b", "cheddar cheese", "Tomatoes"),
            };
            var terms = new[] { "tomato", "cheddar" };
            var result = IngredientMatcher.Filter(recipes, terms, IngredientCombineMode.All).ToList();
            Assert.Single(result);
            Assert.Equal("b", result[0].Name);
        }

        [Fact]
        public void Filter_Any_MatchesIfAnyTerm()
        {
            var recipes = new[]
            {
                R("a", "basil only"),
                R("b", "fresh oregano"),
            };
            var terms = new[] { "basil", "oregano" };
            var result = IngredientMatcher.Filter(recipes, terms, IngredientCombineMode.Any).ToList();
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Filter_Substring_IsCaseInsensitive()
        {
            var recipes = new[] { R("x", "CHICKEN breast") };
            var result = IngredientMatcher.Filter(recipes, new[] { "chicken" }, IngredientCombineMode.All).ToList();
            Assert.Single(result);
        }

        [Fact]
        public void Filter_EmptyTerms_YieldsNothing()
        {
            var recipes = new[] { R("x", "salt") };
            var result = IngredientMatcher.Filter(recipes, new string[0], IngredientCombineMode.All).ToList();
            Assert.Empty(result);
        }

        [Fact]
        public void Filter_OrOfAnd_TomatoesAndCheeseOrBasil()
        {
            var recipes = new[]
            {
                R("both", "tomatoes", "cheddar cheese"),
                R("tomOnly", "tomatoes"),
                R("herb", "basil"),
            };
            var groups = new IReadOnlyList<string>[]
            {
                new[] { "tomatoes", "cheddar" },
                new[] { "basil" },
            };
            var result = IngredientMatcher.Filter(recipes, groups).Select(r => r.Name).ToList();
            Assert.Contains("both", result);
            Assert.Contains("herb", result);
            Assert.DoesNotContain("tomOnly", result);
        }
    }
}
