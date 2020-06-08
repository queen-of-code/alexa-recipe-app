using Amazon.DynamoDBv2.Model;
using System;

namespace RecipeAPI.DynamoModels
{
    public interface IDynamoTable
    {
        string TableName { get; }

        CreateTableRequest CreateRequest { get; }

        bool IsValid();

        long EntityId { get; set; }

        DateTime LastUpdateTime { get; set; }
    }
}
