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
        private static readonly object LockObject = new object();

        private const string TableNotCreatedError = "Table wasn't created correctly in Dynamo.";

        public DynamoRecipeService(IAmazonDynamoDB amazonDynamoDb)
        {
            _client = amazonDynamoDb;
        }

        internal void SetupMockContext(IDynamoDBContext context)
        {
            _testContext = context;
        }

        private static async Task<bool> EnsureTableExists(IAmazonDynamoDB client, IDynamoTable table)
        {
            if (table == null || client == null) return false;

            var createRequest = table.CreateRequest;
            var created = await client.CreateTableAsync(createRequest).ConfigureAwait(true);
            return created?.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        /// <summary>
        /// Creates the Recipes table, if it does not already exist.
        /// </summary>
        /// <returns>True if there's a good table to use, false otherwise.</returns>
        public static bool EnsureTablesExists(IAmazonDynamoDB client)
        {
            if (Initialized) return true;
            if (client == null) return false;

            lock(LockObject)
            {
                if (!Initialized)
                {
                    var creates = new List<Task<bool>>();
                    var tables = new IDynamoTable[] { new Recipe(), new Meal(), new Person(), new Plan() };
                    var request = new ListTablesRequest { Limit = 10 };

                    var response = client.ListTablesAsync(request).GetAwaiter().GetResult();
                    foreach (var table in tables)
                    {
                        if (!response.TableNames.Contains(table.TableName))
                        {
                            creates.Add(EnsureTableExists(client, table));
                        }

                    }

                    Task.WaitAll(creates.ToArray());
                    var success = creates.TrueForAll(s => s.Result);

                    Initialized = true;

                    return success;
                }
            }

            return true;
        }


        public async Task<T> RetrieveItem<T>(string userId, long itemId) where T : IDynamoTable, new()
        {
            if (!Initialized)
            {
                var result = EnsureTablesExists(_client);
                if (!result) throw new ArgumentException(TableNotCreatedError);
            }

            using (var context = _testContext ?? new DynamoDBContext(_client))
            {
                try
                {
                    return await context.LoadAsync<T>(userId, itemId).ConfigureAwait(false);
                }
                catch (AmazonDynamoDBException)
                {
                    return default(T);
                }
            }
        }

        /// <summary>
        /// Saves an item into the database. Returns false if something went wrong, or
        /// true if it was saved successfully.
        /// </summary>
        public async Task<bool> SaveItem<T>(T item) where T : IDynamoTable, new()
        {
            if (!Initialized)
            {
                var result = EnsureTablesExists(_client);
                if (!result) throw new ArgumentException(TableNotCreatedError);
            }

            if (item == null) return false;

            using (var context = _testContext ?? new DynamoDBContext(_client))
            {
                item.LastUpdateTime = DateTime.UtcNow;
                if (item.EntityId == default(long))
                {
                    item.EntityId = Utilities.NextInt64();
                }

                if (!item.IsValid())
                {
                    return false;
                }

                try
                {
                    await context.SaveAsync(item).ConfigureAwait(false);
                    return true;
                }
                catch (AmazonDynamoDBException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Deletes an item from the database. Returns false if something went wrong, or
        /// true if it was deleted successfully.
        /// </summary>
        public async Task<bool> DeleteItem<T>(string userId, long itemId) where T : IDynamoTable, new()
        {
            if (!Initialized)
            {
                var result = EnsureTablesExists(_client);
                if (!result) throw new ArgumentException(TableNotCreatedError);
            }

            using (var context = _testContext ?? new DynamoDBContext(_client))
            {
                try
                {
                    //throw new AccessViolationException("OH NO YOU CANNOT DELETE THIS");
                    await context.DeleteAsync<T>(userId, itemId).ConfigureAwait(false);
                    return true;
                }
                catch (AmazonDynamoDBException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets all of a specified item from the right database for a user. Use with caution!
        /// </summary>
        public async Task<IEnumerable<T>> GetAllItemsForUser<T>(string userId) where T : IDynamoTable, new()
        {
            if (!Initialized)
            {
                var result = EnsureTablesExists(_client);
                if (!result) throw new ArgumentException(TableNotCreatedError);
            }

            using (var context = _testContext ?? new DynamoDBContext(_client))
            {
                try
                {
                    var query = context.QueryAsync<T>(userId);
                    return await query.GetRemainingAsync().ConfigureAwait(false);
                }
                catch (AmazonDynamoDBException)
                {
                    return Array.Empty<T>();
                }
            }
        }

        /// <summary>
        /// Retrieves a recipe from the database. Returns null if nothing could be found.
        /// </summary>
        public async Task<Recipe> RetrieveRecipe(string userId, long recipeId)
        {
            if (!Initialized)
            {
                var result = EnsureTablesExists(_client);
                Initialized = result;
            }

            using (var context = _testContext ?? new DynamoDBContext(_client))
            {
                try
                {
                    return await context.LoadAsync<Recipe>(userId, recipeId).ConfigureAwait(false);
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
                var result = EnsureTablesExists(_client);
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
                    return Array.Empty<Recipe>();
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
                var result = EnsureTablesExists(_client);
                Initialized = result;
            }
            using (var context = _testContext ?? new DynamoDBContext(_client))
            {
                try
                {
                    //throw new AccessViolationException("OH NO YOU CANNOT DELETE THIS");
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
                var result = EnsureTablesExists(_client);
                Initialized = result;
            }

            if (recipe == null)
            {
                return false;
            }

            using (var context = _testContext ?? new DynamoDBContext(_client))
            {
                recipe.LastUpdateTime = DateTime.UtcNow;
                if (recipe.EntityId == default(long))
                {
                    recipe.EntityId = Utilities.NextInt64();
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
