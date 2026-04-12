using System;
using System.Collections.Generic;

using Google.Cloud.Firestore;

namespace RecipeAPI.FirestoreModels
{
    [FirestoreData]
    public sealed class Person : IFirestoreEntity
    {
        public const string CollectionName = "persons";

        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public Timestamp LastUpdateTime { get; set; }

        [FirestoreProperty]
        public float ServingAdjustment { get; set; } = 1.0F;

        [FirestoreProperty]
        public List<string> Restrictions { get; set; } = new List<string>();

        public Person() { }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(UserId)
                && !string.IsNullOrWhiteSpace(Id)
                && !string.IsNullOrWhiteSpace(Name);
        }
    }
}
