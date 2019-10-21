using System;

using Xunit;

namespace Integration
{
    [Trait("Category", "Integration")]
    public abstract class IntegrationTestBase
    {
        public abstract string LocalBaseUrl { get; }
        public abstract string QaBaseUrl { get; }
        public abstract string ProdUrl { get; }

        public string GetTestUrl(string overrideEnv = null)
        {
            var env = Environment.GetEnvironmentVariable("RecipeEnv");
            if (!string.IsNullOrEmpty(overrideEnv))
            {
                env = overrideEnv;
            }

            if (string.IsNullOrWhiteSpace(env) || string.Equals("local", env))
            {
                return LocalBaseUrl;
            }

            if (string.Equals(env, "QA", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(env, "staging", StringComparison.OrdinalIgnoreCase))
            {
                return QaBaseUrl;
            }

            return ProdUrl;
        }
    }
}
