using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;

namespace RecipeAPI.DynamoModels
{
    [DynamoDBTable(PersonTableName)]
    public sealed class Person : IDynamoTable
    {
        [DynamoDBIgnore]
        public const string PersonTableName = "Person";
        [DynamoDBIgnore]
        public string TableName => PersonTableName;

        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBRangeKey]
        public long EntityId { get; set; }

        [DynamoDBProperty]
        public DateTime LastUpdateTime { get; set; }

        [DynamoDBProperty]
        public string Name { get; set; }

        [DynamoDBProperty]
        public float ServingAdjustment { get; set; } = 1.0F;

        [DynamoDBProperty]
        public List<string> Restrictions { get; set; } = new List<string>();

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

        public Person()
        {
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(this.UserId) || this.EntityId == default(long))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.Name))
            {
                return false;
            }

            return true;
        }
    }
}
