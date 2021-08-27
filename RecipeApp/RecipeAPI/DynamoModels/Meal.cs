using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;

namespace RecipeAPI.DynamoModels
{
    [DynamoDBTable(MealTableName)]
    public sealed class Meal : IDynamoTable
    {
        [DynamoDBIgnore]
        public const string MealTableName = "Meal";
        [DynamoDBIgnore]
        public string TableName => MealTableName;

        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBRangeKey]
        public long EntityId { get; set; }

        [DynamoDBProperty]
        public string MealName { get; set; }

        [DynamoDBProperty]
        public List<long> Recipes { get; } = new List<long>();

        [DynamoDBProperty]
        public int Servings { get; set; }

        [DynamoDBProperty]
        public int PrepTimeMins { get; set; }

        [DynamoDBProperty]
        public List<long> FavoriteOfUsers { get; } = new List<long>();

        [DynamoDBProperty]
        public List<string> Allergens { get; } = new List<string>();

        [DynamoDBProperty]
        public DateTime LastUpdateTime { get; set; }

        [DynamoDBIgnore]
        public CreateTableRequest CreateRequest =>
            new CreateTableRequest
            {
                TableName = TableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = nameof(UserId), AttributeType = "S" },
                    new AttributeDefinition { AttributeName = nameof(EntityId), AttributeType = "N" }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement { AttributeName = nameof(UserId), KeyType = "HASH" },
                    new KeySchemaElement { AttributeName = nameof(EntityId), KeyType = "RANGE" }
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                }
            };

        public Meal()
        {
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(this.UserId) || this.EntityId == default(long))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.MealName))
            {
                return false;
            }

            return true;
        }
    }
}
