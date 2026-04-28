using System;
using System.Collections.Generic;
using System.Linq;

using Google.Cloud.Firestore;

using RecipeApp.Core.ExternalModels;

namespace RecipeAPI.FirestoreModels
{
    [FirestoreData]
    public sealed class Recipe : IEquatable<Recipe>, IFirestoreEntity
    {
        public const string CollectionName = "recipes";

        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public Timestamp LastUpdateTime { get; set; }

        [FirestoreProperty]
        public List<string> Ingredients { get; set; } = new List<string>();

        [FirestoreProperty]
        public List<string> Steps { get; set; } = new List<string>();

        [FirestoreProperty]
        public int Servings { get; set; }

        [FirestoreProperty]
        public int PrepTimeMins { get; set; }

        [FirestoreProperty]
        public int CookTimeMins { get; set; }

        [FirestoreProperty]
        public int VersionNumber { get; set; }

        [FirestoreProperty]
        public string CompletedImageUrl { get; set; }

        public Recipe() { }

        public Recipe(RecipeModel external)
        {
            if (external == null) return;
            CookTimeMins = external.CookTimeMins;
            LastUpdateTime = Timestamp.FromDateTime(external.LastUpdateTime == default
                ? DateTime.UtcNow
                : DateTime.SpecifyKind(external.LastUpdateTime, DateTimeKind.Utc));
            Name = external.Name;
            PrepTimeMins = external.PrepTimeMins;
            Id = external.RecipeId;
            Servings = external.Servings;
            UserId = external.UserId;
            Steps = new List<string>(external.Steps);
            Ingredients = new List<string>(external.Ingredients);
            CompletedImageUrl = external.CompletedImageUrl;
        }

        public RecipeModel GenerateExternalRecipe()
        {
            var recipe = new RecipeModel
            {
                UserId = UserId,
                RecipeId = Id,
                Name = Name,
                LastUpdateTime = LastUpdateTime.ToDateTime(),
                PrepTimeMins = PrepTimeMins,
                CookTimeMins = CookTimeMins,
                Servings = Servings
            };
            recipe.Steps.AddRange(Steps.Select(s => s));
            recipe.Ingredients.AddRange(Ingredients.Select(s => s));
            recipe.CompletedImageUrl = CompletedImageUrl;
            return recipe;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(UserId)
                && !string.IsNullOrWhiteSpace(Id)
                && !string.IsNullOrWhiteSpace(Name)
                && Ingredients != null
                && Steps != null;
        }

        public override bool Equals(object otherObj)
        {
            var other = otherObj as Recipe;
            return other != null && UserId == other.UserId && Id == other.Id;
        }

        bool IEquatable<Recipe>.Equals(Recipe other) => Equals(other);

        public override int GetHashCode() => HashCode.Combine(UserId, Id);
    }
}
