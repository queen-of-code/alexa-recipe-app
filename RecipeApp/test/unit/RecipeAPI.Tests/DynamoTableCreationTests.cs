using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Moq;
using Xunit;

namespace RecipeAPI.Tests
{
    [Collection("DynamoTests")]
    [CollectionDefinition("DynamoTests", DisableParallelization = true)]
    [Trait("Category", "Unit")]
    public class DynamoTableCreationTests
    {
        [Fact]
        public void EnsureTableExists_True()
        {
            DynamoRecipeService.Initialized = false;
            var moq = new Mock<IAmazonDynamoDB>();
            moq.SetupAllProperties();
            moq.Setup(s =>
                s.ListTablesAsync(It.IsAny<ListTablesRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ListTablesResponse { TableNames = new List<string> { "Recipe", "Person", "Meal", "Plan", "Whatever" }, HttpStatusCode = HttpStatusCode.OK });

            var exists = DynamoRecipeService.EnsureTablesExists(moq.Object);
            Assert.True(exists);
        }

        [Fact]
        public void EnsureTableExists_Missing()
        {
            DynamoRecipeService.Initialized = false;
            var moq = new Mock<IAmazonDynamoDB>();
            moq.SetupAllProperties();
            moq.Setup(s =>
                s.ListTablesAsync(It.IsAny<ListTablesRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ListTablesResponse { TableNames = new List<string> { "Recipe", "Person", "Meal" }, HttpStatusCode = HttpStatusCode.OK });
            moq.Setup(s =>
                s.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new CreateTableResponse { HttpStatusCode = HttpStatusCode.BadRequest });

            var exists = DynamoRecipeService.EnsureTablesExists(moq.Object);
            Assert.False(exists);
        }

        [Fact]
        public void EnsureTableExists_False()
        {
            DynamoRecipeService.Initialized = false;
            var moq = new Mock<IAmazonDynamoDB>();
            moq.SetupAllProperties();
            moq.Setup(s =>
                s.ListTablesAsync(It.IsAny<ListTablesRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ListTablesResponse { TableNames = new List<string> { "NOTHING" } });
            moq.Setup(s =>
                s.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new CreateTableResponse { HttpStatusCode = HttpStatusCode.BadRequest });

            var exists = DynamoRecipeService.EnsureTablesExists(moq.Object);
            Assert.False(exists);
        }

        [Fact]
        public void EnsureTableExists_Created()
        {
            DynamoRecipeService.Initialized = false;
            var moq = new Mock<IAmazonDynamoDB>();
            moq.SetupAllProperties();
            moq.Setup(s =>
                s.ListTablesAsync(It.IsAny<ListTablesRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ListTablesResponse { TableNames = new List<string> { "NOTHING" } });
            moq.Setup(s =>
                s.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new CreateTableResponse { HttpStatusCode = HttpStatusCode.OK });

            var exists = DynamoRecipeService.EnsureTablesExists(moq.Object);
            Assert.True(exists);
        }
    }
}
