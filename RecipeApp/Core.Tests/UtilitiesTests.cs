using System;
using Xunit;

namespace RecipeApp.API.Tests
{
    [Trait("Category", "Unit")]
    public class UtilitiesTests
    {
        [Fact]
        public void NextInt64()
        {
            var int1 = Core.Utilities.NextInt64();
            var int2 = Core.Utilities.NextInt64();

            Assert.NotEqual(int1, int2);
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData(null, null, "")]
        public void GenerateJWT_Nulls(string userId, string issuer, string key)
        {
            Assert.Throws<ArgumentNullException>(() => Core.Utilities.GenerateJWTToken(userId, issuer, key));
        }

        [Theory]
        [InlineData("", "", "")]
        public void GenerateJWT_Empty(string userId, string issuer, string key)
        {
            Assert.Throws<ArgumentException>(() => Core.Utilities.GenerateJWTToken(userId, issuer, key));
        }

        [Theory]
        [InlineData("myUserId", "www.me.com", "12345678901234567890123456784567")]
        public void GenerateJWT_Good(string userId, string issuer, string key)
        {
            var result = Core.Utilities.GenerateJWTToken(userId, issuer, key);
            Assert.NotNull(result);
        }
    }
}
