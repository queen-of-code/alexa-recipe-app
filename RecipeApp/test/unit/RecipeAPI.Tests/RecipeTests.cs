using RecipeAPI.DynamoModels;
using RecipeApp.Core.ExternalModels;
using System;
using Xunit;

namespace RecipeAPI.Tests
{
    public class RecipeTests
    {
        [Fact]
        public void CopyConstructor_Copied()
        {
            var updateTime = DateTime.UtcNow;

            var external = new RecipeModel
            {
                CookTimeMins = 1,
                LastUpdateTime = updateTime,
                Name = "SOme Name",
                PrepTimeMins = 2,
                RecipeId = 3,
                Servings = 4,
                UserId = "5"
            };
            external.Ingredients.Add("carrots");
            external.Steps.Add("do something");

            var copy = new Recipe(external);

            Assert.NotNull(copy);
            Assert.Equal(external.CookTimeMins, copy.CookTimeMins);
            Assert.Equal(external.LastUpdateTime, copy.LastUpdateTime);
            Assert.Equal(external.Name, copy.Name);
            Assert.Equal(external.PrepTimeMins, copy.PrepTimeMins);
            Assert.Equal(external.RecipeId, copy.RecipeId);
            Assert.Equal(external.Servings, copy.Servings);
            Assert.Equal(external.UserId, copy.UserId);
            Assert.Equal(external.Steps.Count, copy.Steps.Count);
            Assert.Equal(external.Ingredients.Count, copy.Ingredients.Count);

            copy.Steps.Add("profit");
            Assert.Equal(external.Steps.Count + 1, copy.Steps.Count);

            copy.Ingredients.RemoveAt(0);
            Assert.Equal(external.Ingredients.Count - 1, copy.Ingredients.Count);
        }
    }
}
