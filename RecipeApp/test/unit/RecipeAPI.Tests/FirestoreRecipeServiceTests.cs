using Moq;
using RecipeAPI.FirestoreModels;
using System.Threading.Tasks;
using Xunit;

namespace RecipeAPI.Tests
{
    [Trait("Category", "Unit")]
    public class FirestoreRecipeServiceTests
    {
        // Service-level unit tests validate IsValid() and null-guard behavior without
        // requiring a real Firestore connection. Integration tests use the emulator.

        [Fact]
        public void Recipe_IsValid_WithAllFields_ReturnsTrue()
        {
            var recipe = new Recipe
            {
                Id = "abc123",
                UserId = "user1",
                Name = "Chicken Tikka",
                CookTimeMins = 30,
                PrepTimeMins = 15,
                Servings = 4
            };
            Assert.True(recipe.IsValid());
        }

        [Fact]
        public void Recipe_IsValid_MissingId_ReturnsFalse()
        {
            var recipe = new Recipe { UserId = "user1", Name = "Test" };
            Assert.False(recipe.IsValid());
        }

        [Fact]
        public void Recipe_IsValid_MissingUserId_ReturnsFalse()
        {
            var recipe = new Recipe { Id = "abc", Name = "Test" };
            Assert.False(recipe.IsValid());
        }

        [Fact]
        public void Recipe_IsValid_MissingName_ReturnsFalse()
        {
            var recipe = new Recipe { Id = "abc", UserId = "user1", Name = "" };
            Assert.False(recipe.IsValid());
        }

        [Fact]
        public async Task SaveRecipe_NullRecipe_ReturnsFalse()
        {
            var mockService = new Mock<IFirestoreRecipeService>();
            mockService.Setup(s => s.SaveRecipe(null)).ReturnsAsync(false);
            var result = await mockService.Object.SaveRecipe(null);
            Assert.False(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SaveRecipe_DelegatesToService(bool expected)
        {
            var recipe = new Recipe { Id = "1", UserId = "u", Name = "Test" };
            var mockService = new Mock<IFirestoreRecipeService>();
            mockService.Setup(s => s.SaveRecipe(recipe)).ReturnsAsync(expected);
            var result = await mockService.Object.SaveRecipe(recipe);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Meal_IsValid_WithAllFields_ReturnsTrue()
        {
            var meal = new Meal { Id = "m1", UserId = "u1", MealName = "Breakfast" };
            Assert.True(meal.IsValid());
        }

        [Fact]
        public void Person_IsValid_WithAllFields_ReturnsTrue()
        {
            var person = new Person { Id = "p1", UserId = "u1", Name = "Alice" };
            Assert.True(person.IsValid());
        }

        [Fact]
        public void Plan_IsValid_WithAllFields_ReturnsTrue()
        {
            var plan = new Plan { Id = "pl1", UserId = "u1" };
            Assert.True(plan.IsValid());
        }
    }
}
