using Google.Cloud.Firestore;

namespace RecipeAPI.FirestoreModels
{
    [FirestoreData]
    public sealed class Plan : IFirestoreEntity
    {
        public const string CollectionName = "plans";

        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public Timestamp LastUpdateTime { get; set; }

        public Plan() { }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(UserId)
                && !string.IsNullOrWhiteSpace(Id);
        }
    }
}
