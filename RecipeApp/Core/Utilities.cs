using System;
using System.Security.Cryptography;

namespace RecipeApp.Core
{
    public static class Utilities
    {
        private static readonly RNGCryptoServiceProvider Random = new RNGCryptoServiceProvider();

        public static Int64 NextInt64()
        {
            var bytes = new byte[sizeof(Int64)];
            Random.GetBytes(bytes);
            return BitConverter.ToInt64(bytes , 0);
        }
    }
}
