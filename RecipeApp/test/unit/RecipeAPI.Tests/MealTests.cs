using RecipeAPI.DynamoModels;
using System;
using Xunit;

namespace RecipeAPI.Tests
{
    [Trait("Category", "Unit")]
    public class MealTests
    {
        private static readonly Meal Meal1 = new Meal
        {
            LastUpdateTime = DateTime.UtcNow,
            MealName = "Meal123",
            PrepTimeMins = 2,
            EntityId = 123,
            Servings = 4,
            UserId = "5"
        };

        [Fact]
        public void IsValid_Valid()
        {
            var result = Meal1.IsValid();
            Assert.True(result);
        }
    }
}
