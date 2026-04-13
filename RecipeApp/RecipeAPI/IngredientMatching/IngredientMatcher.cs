using System;
using System.Collections.Generic;
using System.Linq;

using RecipeAPI.FirestoreModels;

namespace RecipeAPI.IngredientMatching
{
    /// <summary>
    /// Matches recipes by ingredient lines using case-insensitive substring checks.
    /// Internal shape: OR of AND groups — each inner list is AND; outer list is OR.
    /// Flat "All" = one group with all terms; flat "Any" = one term per group.
    /// </summary>
    public static class IngredientMatcher
    {
        /// <summary>
        /// Filters recipes where the ingredient lines satisfy the OR-of-AND term groups.
        /// </summary>
        public static IEnumerable<Recipe> Filter(
            IEnumerable<Recipe> recipes,
            IReadOnlyList<IReadOnlyList<string>> orOfAndSegments)
        {
            if (recipes == null || orOfAndSegments == null || orOfAndSegments.Count == 0)
                return Array.Empty<Recipe>();

            foreach (var seg in orOfAndSegments)
            {
                if (seg == null || seg.Count == 0)
                    return Array.Empty<Recipe>();
            }

            return recipes.Where(r => orOfAndSegments.Any(segment => SegmentMatches(r, segment)));
        }

        /// <summary>
        /// Flat All / Any over <paramref name="terms"/> (structured API).
        /// </summary>
        public static IEnumerable<Recipe> Filter(
            IEnumerable<Recipe> recipes,
            IReadOnlyList<string> terms,
            IngredientCombineMode mode)
        {
            var normalized = NormalizeTerms(terms);
            if (normalized.Count == 0)
                return Array.Empty<Recipe>();

            IReadOnlyList<IReadOnlyList<string>> orOfAnd = mode == IngredientCombineMode.All
                ? new[] { normalized }
                : normalized.Select(t => (IReadOnlyList<string>)new[] { t }).ToList();

            return Filter(recipes, orOfAnd);
        }

        private static bool SegmentMatches(Recipe recipe, IReadOnlyList<string> segmentTerms)
        {
            return segmentTerms.All(t => TermMatches(recipe, t));
        }

        private static bool TermMatches(Recipe recipe, string term)
        {
            if (recipe?.Ingredients == null || string.IsNullOrEmpty(term))
                return false;

            foreach (var line in recipe.Ingredients)
            {
                if (line != null && line.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static List<string> NormalizeTerms(IReadOnlyList<string> terms)
        {
            if (terms == null)
                return new List<string>();

            return terms
                .Select(t => t?.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();
        }
    }
}
