using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;

using Microsoft.Extensions.Logging;

using RecipeAPI.FirestoreModels;

namespace RecipeAPI
{
    public class FirestoreRecipeService : IFirestoreRecipeService
    {
        private readonly FirestoreDb _db;
        private readonly ILogger<FirestoreRecipeService> _logger;
        private readonly string _storageBucket;

        public FirestoreRecipeService(
            FirestoreDb db,
            ILogger<FirestoreRecipeService> logger,
            Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _db = db;
            _logger = logger;
            var projectId = configuration["GCP_PROJECT_ID"] ?? "queen-of-code";
            _storageBucket = configuration["FIREBASE_STORAGE_BUCKET"]
                ?? $"{projectId}.appspot.com";
        }

        // Derives Firestore collection name from the static CollectionName field on T.
        private static string CollectionNameFor<T>()
        {
            var field = typeof(T).GetField("CollectionName",
                BindingFlags.Public | BindingFlags.Static);
            return field?.GetValue(null) as string
                ?? typeof(T).Name.ToLowerInvariant() + "s";
        }

        private CollectionReference UserCollection<T>(string userId) =>
            _db.Collection("users").Document(userId).Collection(CollectionNameFor<T>());

        private CollectionReference RecipesFor(string userId) =>
            _db.Collection("users").Document(userId).Collection(Recipe.CollectionName);

        // ── Recipe-specific methods ──────────────────────────────────────────

        public async Task<Recipe> RetrieveRecipe(string userId, string recipeId)
        {
            var snap = await RecipesFor(userId).Document(recipeId).GetSnapshotAsync();
            return snap.Exists ? snap.ConvertTo<Recipe>() : null;
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesForUser(string userId)
        {
            var snap = await RecipesFor(userId).GetSnapshotAsync();
            return snap.Documents.Select(d => d.ConvertTo<Recipe>());
        }

        public async Task<Recipe> SaveRecipe(Recipe recipe) =>
            await SaveItemCoreAsync(recipe).ConfigureAwait(false);

        public async Task<bool> DeleteRecipe(string userId, string recipeId)
        {
            var existing = await RetrieveRecipe(userId, recipeId).ConfigureAwait(false);
            var objectPath = existing != null
                ? CompletedImageMetadata.TryGetStorageObjectPath(userId, existing.CompletedImageUrl)
                : null;

            if (objectPath != null
                && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FIRESTORE_EMULATOR_HOST")))
            {
                try
                {
                    var storage = await StorageClient.CreateAsync().ConfigureAwait(false);
                    await storage.DeleteObjectAsync(_storageBucket, objectPath).ConfigureAwait(false);
                }
                catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation(
                        "Completed image object already absent: {Bucket}/{Object}",
                        _storageBucket,
                        objectPath);
                }
#pragma warning disable CA1031
                catch (Exception ex)
#pragma warning restore CA1031
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to delete completed image for user {UserId} recipe {RecipeId}; continuing with Firestore delete",
                        userId,
                        recipeId);
                }
            }

            return await DeleteItem<Recipe>(userId, recipeId).ConfigureAwait(false);
        }

        // ── Generic methods ──────────────────────────────────────────────────

        public async Task<T> RetrieveItem<T>(string userId, string itemId)
            where T : class, IFirestoreEntity, new()
        {
            var snap = await UserCollection<T>(userId).Document(itemId).GetSnapshotAsync();
            return snap.Exists ? snap.ConvertTo<T>() : null;
        }

        public async Task<bool> SaveItem<T>(T item) where T : class, IFirestoreEntity, new() =>
            await SaveItemCoreAsync(item).ConfigureAwait(false) != null;

        private async Task<T> SaveItemCoreAsync<T>(T item) where T : class, IFirestoreEntity, new()
        {
            if (item == null)
                return null;

            if (string.IsNullOrWhiteSpace(item.Id))
                item.Id = Guid.NewGuid().ToString("N");

            // Set LastUpdateTime via reflection so we don't need it on the interface.
            var prop = typeof(T).GetProperty("LastUpdateTime");
            if (prop != null && prop.CanWrite)
                prop.SetValue(item, Timestamp.GetCurrentTimestamp());

            if (!item.IsValid())
                return null;

            if (item is Recipe r && !CompletedImageMetadata.IsValid(r.UserId, r.CompletedImageUrl))
                return null;

            await UserCollection<T>(item.UserId).Document(item.Id).SetAsync(item);
            return item;
        }

        public async Task<bool> DeleteItem<T>(string userId, string itemId)
            where T : class, IFirestoreEntity, new()
        {
            await UserCollection<T>(userId).Document(itemId).DeleteAsync();
            return true;
        }

        public async Task<IEnumerable<T>> GetAllItemsForUser<T>(string userId)
            where T : class, IFirestoreEntity, new()
        {
            var snap = await UserCollection<T>(userId).GetSnapshotAsync();
            return snap.Documents.Select(d => d.ConvertTo<T>());
        }
    }
}
