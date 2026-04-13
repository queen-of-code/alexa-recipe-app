using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RecipeAPI.IngredientMatching
{
    /// <summary>
    /// v1 NL heuristic: split on <c> or </c> (case-insensitive) first → OR of segments;
    /// within each segment split on <c> and </c> or comma → AND terms.
    /// </summary>
    public static class IngredientQueryParser
    {
        private static readonly Regex OrSplit = new Regex(@"\s+or\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex AndSplit = new Regex(@"\s+and\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Returns OR-of-AND term groups. Empty input yields an empty outer list (caller should reject).
        /// </summary>
        public static IReadOnlyList<IReadOnlyList<string>> Parse(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Array.Empty<IReadOnlyList<string>>();

            var segments = OrSplit.Split(query.Trim());
            var result = new List<IReadOnlyList<string>>();

            foreach (var segment in segments)
            {
                var terms = SplitAndAndComma(segment);
                if (terms.Count > 0)
                    result.Add(terms);
            }

            return result;
        }

        private static List<string> SplitAndAndComma(string segment)
        {
            var parts = AndSplit.Split(segment);
            var terms = new List<string>();

            foreach (var part in parts)
            {
                foreach (var piece in part.Split(','))
                {
                    var t = piece.Trim();
                    if (t.Length > 0)
                        terms.Add(t);
                }
            }

            return terms;
        }
    }
}
