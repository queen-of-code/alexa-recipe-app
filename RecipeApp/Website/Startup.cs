using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using Website.Authorization;
using Website.Data;

namespace Website
{
    /// <summary>
    /// docker build -t queenofcode/alexa-recipe-app .
    /// docker run -it --rm -p 5000:80 queenofcode/alexa-recipe-app:latest
    /// docker service create --name recipeui --secret Authentication.Microsoft.ApplicationId --secret Authentication.Microsoft.Password --publish published = 5000, target= 80, mode=host queenofcode/alexa-recipe-app:latest
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddHttpClient("RecipeAPI", client =>
            {
                client.BaseAddress = new Uri(Configuration.GetConnectionString("RecipeAPIConnection"));
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddSingleton<RecipeService>();
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = GetSecretOrEnvVar("Authentication.Microsoft.ApplicationId");
                microsoftOptions.ClientSecret = GetSecretOrEnvVar("Authentication.Microsoft.Password");
            });
                
            //    .AddFacebook(facebookOptions =>
            //{
            //    facebookOptions.AppId = GetSecretOrEnvVar("Facebook.API.Key");
            //    facebookOptions.AppSecret = GetSecretOrEnvVar("Facebook.API.Secret");
            //});

            services.AddMvc(options => options.EnableEndpointRouting = false)
                            .AddRazorPagesOptions(options =>
                             {
                                 options.Conventions.AuthorizeFolder("/Recipes");
                                 options.Conventions.AllowAnonymousToPage("/Index");
                                 options.Conventions.AllowAnonymousToPage("/Contact");
                                 options.Conventions.AllowAnonymousToPage("/About");
                             }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            //services.AddMvc(config =>
            //{
            //    // using Microsoft.AspNetCore.Mvc.Authorization;
            //    // using Microsoft.AspNetCore.Authorization;
            //    var policy = new AuthorizationPolicyBuilder()
            //                     .RequireAuthenticatedUser()
            //                     .Build();
            //    config.Filters.Add(new AuthorizeFilter(policy));
            //}).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Authorization handlers.
            services.AddScoped<IAuthorizationHandler, IsRecipeOwnerAuthorizationHandler>();

            services.AddSingleton<IAuthorizationHandler, RecipeAdministratorsAuthorizationHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Console.WriteLine($"Environment is {env.EnvironmentName}");
            if ("Development".Equals(env.EnvironmentName, StringComparison.OrdinalIgnoreCase))
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMvc();
        }

        public string GetSecretOrEnvVar(string key)
        {
            const string DOCKER_SECRET_PATH = "/run/secrets/";
            if (Directory.Exists(DOCKER_SECRET_PATH))
            {
                IFileProvider provider = new PhysicalFileProvider(DOCKER_SECRET_PATH);
                IFileInfo fileInfo = provider.GetFileInfo(key);
                if (fileInfo.Exists)
                {
                    using (var stream = fileInfo.CreateReadStream())
                    using (var streamReader = new StreamReader(stream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }

            var hmm = Configuration.GetValue<string>(key) ?? "MISSING";
            return hmm;
        }
    }
}
