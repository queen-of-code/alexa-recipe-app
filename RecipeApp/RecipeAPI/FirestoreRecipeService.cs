using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Google.Cloud.Firestore;

using RecipeAPI.FirestoreModels;

namespace RecipeAPI
{
    public class FirestoreRecipeService : IFirestoreRecipeService
    {
        private readonly FirestoreDb _db;

        public FirestoreRecipeService(FirestoreDb db)
        {
            _db = db;
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

        public Task<bool> SaveRecipe(Recipe recipe) => SaveItem(recipe);

        public Task<bool> DeleteRecipe(string userId, string recipeId) =>
            DeleteItem<Recipe>(userId, recipeId);

        // ── Generic methods ──────────────────────────────────────────────────

        public async Task<T> RetrieveItem<T>(string userId, string itemId)
            where T : class, IFirestoreEntity, new()
        {
            var snap = await UserCollection<T>(userId).Document(itemId).GetSnapshotAsync();
            return snap.Exists ? snap.ConvertTo<T>() : null;
        }

        public async Task<bool> SaveItem<T>(T item) where T : class, IFirestoreEntity, new()
        {
            if (item == null) return false;

            if (string.IsNullOrWhiteSpace(item.Id))
                item.Id = Guid.NewGuid().ToString("N");

            // Set LastUpdateTime via reflection so we don't need it on the interface.
            var prop = typeof(T).GetProperty("LastUpdateTime");
            if (prop != null && prop.CanWrite)
                prop.SetValue(item, Timestamp.GetCurrentTimestamp());

            if (!item.IsValid()) return false;

            await UserCollection<T>(item.UserId).Document(item.Id).SetAsync(item);
            return true;
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
