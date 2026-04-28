using System;
using System.Linq;

namespace RecipeAPI
{
    /// <summary>
    /// Validation and parsing for optional completed-dish image metadata (see feature/recipe-completed-photo/tech-spec.md).
    /// </summary>
    internal static class CompletedImageMetadata
    {
        public const int MaxLength = 2048;

        /// <summary>
        /// Allowed: null/empty; raw Storage object path under this user; or a Firebase download URL whose object path is under this user.
        /// </summary>
        public static bool IsValid(string userId, string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            if (value.Length > MaxLength)
                return false;

            var prefix = $"users/{userId}/";
            if (value.StartsWith(prefix, StringComparison.Ordinal))
                return true;

            if (!TryParseFirebaseDownloadUrl(value, out _, out var objectPath))
                return false;

            return objectPath.StartsWith(prefix, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns GCS object path for delete, or null if nothing to delete / unrecognized format.
        /// </summary>
        public static string TryGetStorageObjectPath(string userId, string storedValue)
        {
            if (string.IsNullOrWhiteSpace(storedValue))
                return null;

            if (storedValue.StartsWith($"users/{userId}/", StringComparison.Ordinal))
                return storedValue;

            if (TryParseFirebaseDownloadUrl(storedValue, out _, out var objectPath)
                && objectPath.StartsWith($"users/{userId}/", StringComparison.Ordinal))
            {
                return objectPath;
            }

            return null;
        }

        /// <summary>
        /// Parses <c>https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{encodedPath}?...</c>.
        /// </summary>
        private static bool TryParseFirebaseDownloadUrl(string url, out string bucket, out string objectPath)
        {
            bucket = null;
            objectPath = null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
                || uri.Host != "firebasestorage.googleapis.com")
            {
                return false;
            }

            // Path like /v0/b/my-bucket/o/users%2Fuid%2F...
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 5
                || segments[0] != "v0"
                || segments[1] != "b"
                || segments[3] != "o")
            {
                return false;
            }

            bucket = Uri.UnescapeDataString(segments[2]);
            objectPath = Uri.UnescapeDataString(string.Join("/", segments.Skip(4)));
            return !string.IsNullOrEmpty(bucket) && !string.IsNullOrEmpty(objectPath);
        }
    }
}
