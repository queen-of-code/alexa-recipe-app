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
            // In local dev (FIREBASE_AUTH_EMULATOR_HOST set), use a fake token — the emulator
            // accepts any credential. In production, use Application Default Credentials (ADC),
            // which resolves to the Cloud Run service account identity automatically.
            var projectId = Configuration["GCP_PROJECT_ID"] ?? "queen-of-code";
            var isEmulator = !string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable("FIREBASE_AUTH_EMULATOR_HOST"));

            // FirebaseAdmin v3 requires Credential to be set even in emulator mode.
            var appOptions = isEmulator
                ? new AppOptions { ProjectId = projectId, Credential = GoogleCredential.FromAccessToken("owner") }
                : new AppOptions
                {
                    Credential = GoogleCredential.GetApplicationDefault(),
                    ProjectId = projectId
                };

            FirebaseApp.Create(appOptions);

            // Firestore — use EmulatorDetection so it connects to the emulator when
            // FIRESTORE_EMULATOR_HOST is set (local dev), or uses ADC in production.
            var firestoreDb = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOrProduction
            }.Build();
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
