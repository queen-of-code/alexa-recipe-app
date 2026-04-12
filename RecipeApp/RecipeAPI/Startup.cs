using System;

using FirebaseAdmin;

using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RecipeAPI.Auth;

namespace RecipeAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
            Console.WriteLine("Using production environment.");
        }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
            Console.WriteLine("Using development environment.");
        }

        public void ConfigureStagingServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
            Console.WriteLine("Using staging environment.");
        }

        private void ConfigureCommonServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddControllers(o => o.AllowEmptyInputInBodyModelBinding = true);
            services.AddHealthChecks();

            // CORS — allow the React SPA to call this API
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.WithOrigins(
                        "http://localhost:5173",            // Vite dev server
                        "https://qoc-recipe-app.web.app"   // Firebase Hosting
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });

            // Firebase Admin SDK.
            // When FIREBASE_AUTH_EMULATOR_HOST is set (local dev), skip credential file — emulator
            // handles auth without real GCP credentials. In production, use the mounted SA key.
            var projectId = Configuration["GCP_PROJECT_ID"] ?? "queen-of-code";
            var isEmulator = !string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable("FIREBASE_AUTH_EMULATOR_HOST"));

            var appOptions = isEmulator
                ? new AppOptions { ProjectId = projectId }
                : new AppOptions
                {
                    Credential = GoogleCredential.FromFile(
                        Configuration["GOOGLE_APPLICATION_CREDENTIALS"]),
                    ProjectId = projectId
                };

            FirebaseApp.Create(appOptions);

            // Firestore — automatically uses FIRESTORE_EMULATOR_HOST if set.
            var firestoreDb = FirestoreDb.Create(projectId);
            services.AddSingleton(firestoreDb);
            services.AddSingleton<IFirestoreRecipeService, FirestoreRecipeService>();

            // Firebase Auth middleware
            services.AddAuthentication("Firebase")
                .AddScheme<AuthenticationSchemeOptions, FirebaseAuthHandler>("Firebase", _ => { });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if ("development".Equals(env.EnvironmentName, StringComparison.OrdinalIgnoreCase))
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
