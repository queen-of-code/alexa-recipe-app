using System;
using System.Collections.Generic;

using Google.Cloud.Firestore;

namespace RecipeAPI.FirestoreModels
{
    [FirestoreData]
    public sealed class Meal : IFirestoreEntity
    {
        public const string CollectionName = "meals";

        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string MealName { get; set; }

        [FirestoreProperty]
        public List<string> Recipes { get; set; } = new List<string>();

        [FirestoreProperty]
        public int Servings { get; set; }

        [FirestoreProperty]
        public int PrepTimeMins { get; set; }

        [FirestoreProperty]
        public List<string> FavoriteOfUsers { get; set; } = new List<string>();

        [FirestoreProperty]
        public List<string> Allergens { get; set; } = new List<string>();

        [FirestoreProperty]
        public Timestamp LastUpdateTime { get; set; }

        public Meal() { }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(UserId)
                && !string.IsNullOrWhiteSpace(Id)
                && !string.IsNullOrWhiteSpace(MealName);
        }
    }
}
