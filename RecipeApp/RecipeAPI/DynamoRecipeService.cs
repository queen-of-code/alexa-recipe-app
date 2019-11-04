using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

using RecipeAPI.DynamoModels;

using RecipeApp.Core;

[assembly: InternalsVisibleTo("RecipeAPI.Tests")]
namespace RecipeAPI
{
    public class DynamoRecipeService : IDynamoRecipeService
    {
        private readonly IAmazonDynamoDB _client;
        private IDynamoDBContext _testContext;
        internal static volatile bool Initialized;

        public DynamoRecipeService(IAmazonDynamoDB amazonDynamoDb)
        {
            _client = amazonDynamoDb;
        }

        internal void SetupMockContext(IDynamoDBContext context)
        {
            _testContext = context;
        }

        /// <summary>
        /// Creates the Recipes table, if it does not already exist.
        /// </summary>
        /// <returns>True if there's a good table to use, false otherwise.</returns>
        public static async Task<bool> EnsureTableExists(IAmazonDynamoDB client)
        {
            if (Initialized)
            {
                return true;
            }

            var request = new ListTablesRequest { Limit = 10 };
            var response = await client.ListTablesAsync(request);

            if (!response.TableNames.Contains("Recipe"))
            {
                var createRequest = new CreateTableRequest
                {
                    TableName = "Recipe",
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "UserId",
                            AttributeType = "S"
                        },
                         new AttributeDefinition
                        {
                            AttributeName = "RecipeId",
                            AttributeType = "N"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "UserId",
                            KeyType = "HASH"
                        },
                        new KeySchemaElement
                        {
                            AttributeName = "RecipeId",
                            KeyType = "RANGE"                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 5,
                        WriteCapacityUnits = 5
                    }
                };

                var created = await client.CreateTableAsync(createRequest);
                Initialized = created.HttpStatusCode == System.Net.HttpStatusCode.OK;
                return created.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }

            Initialized = true;
            return true; // It already existed.
        }


        /// <summary>
        /// Retrieves a recipe from the database. Returns null if nothing could be found.
        /// </summary>
        public async Task<Recipe> RetrieveRecipe(string userId, long recipeId)
        {
            if (!Initialized)
            {
                var result = await EnsureTableExists(_client);
                Initialized = result;
            }

            using (var context = _testContext ?? new DynamoDBContext(_client))
            {
                try
                {
                    return await context.LoadAsync<Recipe>(userId, recipeId);
                }
                catch (AmazonDynamoDBException)
                {
                    return null;
                }
            }
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesForUser(string userId)
        {
            if (!Initialized)
            {
                var result = await EnsureTableExists(_client);
                Initialized = result;
            }

            using (var context = _testContext ?? new DynamoDBContext(_client))
            {
                try
                {
                    var query = context.QueryAsync<Recipe>(userId);
                    return await query.GetRemainingAsync();
                }
                catch (AmazonDynamoDBException)
                {
                    return new Recipe[0];
                }
            }
        }

        /// <summary>
        /// Deletes a recipe from the database. Returns false if something went wrong, or
        /// true if it was deleted successfully.
        /// </summary>
        public async Task<bool> DeleteRecipe(string userId, long recipeId)
        {
            if (!Initialized)
            {
                var result = await EnsureTableExists(_client);
                Initialized = result;
            }

            using (var context = _testContext ?? new DynamoDBContext(_client))
            {
                try
                {
                    await context.DeleteAsync<Recipe>(userId, recipeId);
                    return true;
                }
                catch (AmazonDynamoDBException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Saves a recipe into the database. Returns false if something went wrong, or
        /// true if it was saved successfully.
        /// </summary>
        public async Task<bool> SaveRecipe(Recipe recipe)
        {
            if (!Initialized)
            {
                var result = await EnsureTableExists(_client);
                Initialized = result;
            }

            if (recipe == null)
            {
                return false;
            }

            using (var context = _testContext ?? new DynamoDBContext(_client))
            {
                recipe.LastUpdateTime = DateTime.UtcNow;
                if (recipe.RecipeId == default(long))
                {
                    recipe.RecipeId = Utilities.NextInt64();
                }

                if (!recipe.IsValid())
                {
                    return false;
                }

                try
                {
                    await context.SaveAsync(recipe);
                    return true;
                }
                catch (AmazonDynamoDBException)
                {
                    return false;
                }
            }
        }
    }
}
