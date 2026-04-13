using System.Linq;
using RecipeAPI.IngredientMatching;
using Xunit;

namespace RecipeAPI.Tests
{
    [Trait("Category", "Unit")]
    public class IngredientQueryParserTests
    {
        [Fact]
        public void Parse_AndQuery_SingleSegment()
        {
            var g = IngredientQueryParser.Parse("tomatoes and cheddar");
            Assert.Single(g);
            Assert.Equal(new[] { "tomatoes", "cheddar" }, g[0].ToArray());
        }

        [Fact]
        public void Parse_OrQuery_TwoSegments()
        {
            var g = IngredientQueryParser.Parse("tomatoes or cheddar");
            Assert.Equal(2, g.Count);
            Assert.Single(g[0]);
            Assert.Equal("tomatoes", g[0].First());
            Assert.Single(g[1]);
            Assert.Equal("cheddar", g[1].First());
        }

        [Fact]
        public void Parse_Comma_AllAndTerms()
        {
            var g = IngredientQueryParser.Parse("tomatoes, cheddar");
            Assert.Single(g);
            Assert.Equal(new[] { "tomatoes", "cheddar" }, g[0].ToArray());
        }

        [Fact]
        public void Parse_OrOfAnd_Mixed()
        {
            var g = IngredientQueryParser.Parse("tomatoes and cheddar or basil");
            Assert.Equal(2, g.Count);
            Assert.Equal(new[] { "tomatoes", "cheddar" }, g[0].ToArray());
            Assert.Equal(new[] { "basil" }, g[1].ToArray());
        }
    }
}
