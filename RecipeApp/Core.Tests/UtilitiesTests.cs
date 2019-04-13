using Xunit;

namespace RecipeApp.API.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void NextInt64()
        {
            var int1 = Core.Utilities.NextInt64();
            var int2 = Core.Utilities.NextInt64();

            Assert.NotEqual(int1, int2);
        }
    }
}
