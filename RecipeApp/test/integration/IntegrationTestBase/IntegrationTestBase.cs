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

            string url;
            if (string.IsNullOrWhiteSpace(env) || string.Equals("local", env))
            {
                url =  LocalBaseUrl;
            }
            else if (string.Equals(env, "QA", StringComparison.OrdinalIgnoreCase) || 
                     string.Equals(env, "staging", StringComparison.OrdinalIgnoreCase))
            {
                url = QaBaseUrl;
            }
            else
            {
                url = ProdUrl;
            }

            Console.WriteLine($"Test environment is {env} hitting {url}");
            return url;
        }
    }
}
