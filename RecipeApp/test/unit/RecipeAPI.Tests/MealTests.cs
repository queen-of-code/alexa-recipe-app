using RecipeAPI.FirestoreModels;
using Xunit;

namespace RecipeAPI.Tests
{
    [Trait("Category", "Unit")]
    public class MealTests
    {
        private static readonly Meal Meal1 = new Meal
        {
            Id = "meal-123",
            MealName = "Meal123",
            PrepTimeMins = 2,
            Servings = 4,
            UserId = "5"
        };

        [Fact]
        public void IsValid_Valid()
        {
            Assert.True(Meal1.IsValid());
        }

        [Fact]
        public void IsValid_MissingName_ReturnsFalse()
        {
            var meal = new Meal { Id = "m1", UserId = "u1", MealName = "" };
            Assert.False(meal.IsValid());
        }

        [Fact]
        public void IsValid_MissingId_ReturnsFalse()
        {
            var meal = new Meal { UserId = "u1", MealName = "Breakfast" };
            Assert.False(meal.IsValid());
        }
    }
}
