using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using RecipeApp.Core.ExternalModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecipeAPI.DynamoModels
{
    [DynamoDBTable(RecipeTableName)]
    public sealed class Recipe : IEquatable<Recipe>, IDynamoTable
    {
        [DynamoDBIgnore]
        public const string RecipeTableName = "Recipe";

        [DynamoDBIgnore]
        public string TableName => RecipeTableName;

        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBRangeKey]
        public long EntityId { get; set; }

        [DynamoDBProperty]
        public string Name { get; set; }

        [DynamoDBProperty]
        public DateTime LastUpdateTime { get; set; }

        [DynamoDBProperty]
        public List<string> Ingredients { get; set; } = new List<string>();

        [DynamoDBProperty]
        public List<string> Steps { get; set; } = new List<string>();

        [DynamoDBProperty]
        public int Servings { get; set; }

        [DynamoDBProperty]
        public int PrepTimeMins { get; set; }

        [DynamoDBProperty]
        public int CookTimeMins { get; set; }

        //[DynamoDBVersion]
        public int? VersionNumber { get; set; }

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


        public Recipe()
        {
        }

        public Recipe(RecipeModel external)
        {
            if (external != null)
            {
                this.CookTimeMins = external.CookTimeMins;
                this.LastUpdateTime = external.LastUpdateTime;
                this.Name = external.Name;
                this.PrepTimeMins = external.PrepTimeMins;
                this.EntityId = external.RecipeId;
                this.Servings = external.Servings;
                this.UserId = external.UserId;

                this.Steps = new List<string>(external.Steps);
                this.Ingredients = new List<string>(external.Ingredients);
            }
        }

        /// <summary>
        /// Converts the internal database format of our recipe object into the external
        /// format that is meant to be consumed by the UI or over the API.
        /// </summary>
        public RecipeModel GenerateExternalRecipe()
        {
            var recipe = new RecipeModel
            {
                UserId = this.UserId,
                RecipeId = this.EntityId,
                Name = this.Name,
                LastUpdateTime = this.LastUpdateTime,
                PrepTimeMins = this.PrepTimeMins,
                CookTimeMins = this.CookTimeMins,
                Servings = this.Servings
            };
            recipe.Steps.AddRange(this.Steps.Select(s => s));
            recipe.Ingredients.AddRange(this.Ingredients.Select(s => s));

            return recipe;
        }

        public bool IsValid()
        {
            if (String.IsNullOrWhiteSpace(this.UserId) || this.EntityId == default(long))
            {
                return false;
            }

            if (String.IsNullOrWhiteSpace(this.Name))
            {
                return false;
            }

            if (this.Ingredients == null || this.Steps == null)
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object otherObj)
        {
            var other = otherObj as Recipe;

            if (other == null)
            {
                return false;
            }

            return this.UserId == other.UserId &&
                   this.EntityId == other.EntityId &&
                   this.VersionNumber == other.VersionNumber;
        }

        bool IEquatable<Recipe>.Equals(Recipe other)
        {
            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
    }
}
