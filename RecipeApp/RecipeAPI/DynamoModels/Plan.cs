using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;

namespace RecipeAPI.DynamoModels
{
    [DynamoDBTable(PlanTableName)]
    public sealed class Plan : IDynamoTable
    {
        [DynamoDBIgnore]
        public const string PlanTableName = "Plan";
        [DynamoDBIgnore]
        public string TableName => PlanTableName;

        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBRangeKey]
        public long EntityId { get; set; }

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

        public Plan()
        {
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(this.UserId) || this.EntityId == default(long))
            {
                return false;
            }

            return true;
        }
    }
}
