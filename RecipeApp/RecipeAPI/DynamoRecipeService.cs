using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using RecipeAPI.DynamoModels;
using RecipeApp.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipeAPI
{
    public class DynamoRecipeService
    {
        private readonly IAmazonDynamoDB _client;

        public DynamoRecipeService(IAmazonDynamoDB amazonDynamoDb)
        {
            _client = amazonDynamoDb;
        }

        /// <summary>
        /// Creates the Recipes table, if it does not already exist.
        /// </summary>
        /// <returns>True if there's a good table to use, false otherwise.</returns>
        public static async Task<bool> EnsureTableExists(IAmazonDynamoDB client)
        {
            Console.WriteLine($"Dynamo URL is {client.Config.ServiceURL}");
            Console.WriteLine($"Dynamo RegionName is {client.Config.RegionEndpointServiceName}");
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
                return created.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }

            return true; // It already existed.
        }


        /// <summary>
        /// Retrieves a recipe from the database. Returns null if nothing could be found.
        /// </summary>
        public async Task<Recipe> RetrieveRecipe(string userId, long recipeId)
        {
            var context = new DynamoDBContext(_client);

            try
            {
                return await context.LoadAsync<Recipe>(userId, recipeId);
            }
            catch (AmazonDynamoDBException)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesForUser(string userId)
        {
            var context = new DynamoDBContext(_client);

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

        /// <summary>
        /// Deletes a recipe from the database. Returns false if something went wrong, or
        /// true if it was deleted successfully.
        /// </summary>
        public async Task<bool> DeleteRecipe(string userId, long recipeId)
        {
            var context = new DynamoDBContext(_client);

            try
            {
                await context.DeleteAsync<Recipe>(userId, recipeId, new System.Threading.CancellationToken());
                return true;
            }
            catch (AmazonDynamoDBException)
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes a recipe from the database. Returns false if something went wrong, or
        /// true if it was deleted successfully.
        /// </summary>
        public async Task<bool> DeleteRecipe(Recipe recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            var context = new DynamoDBContext(_client);

            try
            {
                await context.DeleteAsync(recipe);
                return true;
            }
            catch (AmazonDynamoDBException)
            {
                return false;
            }
        }

        /// <summary>
        /// Saves a recipe into the database. Returns false if something went wrong, or
        /// true if it was saved successfully.
        /// </summary>
        public async Task<bool> SaveRecipe(Recipe recipe)
        {
            if (recipe == null || !recipe.IsValid())
            {
                return false;
            }

            var context = new DynamoDBContext(_client);

            recipe.LastUpdateTime = DateTime.UtcNow;
            if (recipe.RecipeId == default(long))
            {
                recipe.RecipeId = Utilities.NextInt64();
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
