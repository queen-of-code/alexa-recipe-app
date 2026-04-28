using Xunit;

namespace RecipeAPI.Tests
{
    [Trait("Category", "Unit")]
    public class CompletedImageMetadataTests
    {
        [Theory]
        [InlineData("users/u1/recipes/r1/completed.jpg")]
        [InlineData("")]
        [InlineData(null)]
        public void IsValid_AcceptsPathOrEmpty(string value)
        {
            Assert.True(CompletedImageMetadata.IsValid("u1", value));
        }

        [Fact]
        public void IsValid_RejectsOtherUserPath()
        {
            Assert.False(CompletedImageMetadata.IsValid("u1", "users/u2/recipes/r1/completed.jpg"));
        }

        [Fact]
        public void IsValid_AcceptsFirebaseDownloadUrlForSameUser()
        {
            var url = "https://firebasestorage.googleapis.com/v0/b/myapp.appspot.com/o/users%2Fu1%2Frecipes%2Fr1%2Fcompleted.jpg?alt=media&token=x";
            Assert.True(CompletedImageMetadata.IsValid("u1", url));
        }

        [Fact]
        public void IsValid_RejectsDownloadUrlForOtherUser()
        {
            var url = "https://firebasestorage.googleapis.com/v0/b/myapp.appspot.com/o/users%2Fu2%2Frecipes%2Fr1%2Fcompleted.jpg?alt=media";
            Assert.False(CompletedImageMetadata.IsValid("u1", url));
        }

        [Fact]
        public void TryGetStorageObjectPath_FromRawPath()
        {
            var p = CompletedImageMetadata.TryGetStorageObjectPath("u1", "users/u1/recipes/r1/completed.webp");
            Assert.Equal("users/u1/recipes/r1/completed.webp", p);
        }

        [Fact]
        public void TryGetStorageObjectPath_FromDownloadUrl()
        {
            var url = "https://firebasestorage.googleapis.com/v0/b/b.appspot.com/o/users%2Fu1%2Fx%2Fy.jpg?alt=media";
            Assert.Equal("users/u1/x/y.jpg", CompletedImageMetadata.TryGetStorageObjectPath("u1", url));
        }
    }
}
