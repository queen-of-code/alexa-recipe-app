using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

using Amazon.DynamoDBv2;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true; // More detailed logging locally.
            var key = Configuration["RecipeConnectionKey"];
            Console.WriteLine($"Key starts with {key.Substring(0, 4)}");
            Console.WriteLine("Using development environment.");
        }

        public void ConfigureStagingServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true; // More detailed logging in Staging
            var key = Configuration["RecipeConnectionKey"];
            Console.WriteLine($"Key starts with {key.Substring(0, 4)}");
            Console.WriteLine("Using staging environment.");
        }

        private void ConfigureCommonServices(IServiceCollection services)
        {
            services.AddMvc();

            var awsOptions = Configuration.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonDynamoDB>(awsOptions);
            services.AddControllers(o => o.AllowEmptyInputInBodyModelBinding = true);
            services.AddHealthChecks();

            // Add JWT Authentication 
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["JwtIssuer"],
                        ValidAudience = "API",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["RecipeConnectionKey"])),
                        ClockSkew = TimeSpan.FromSeconds(60) // remove delay of token when expire
                    };
                });


            services.AddSingleton<IDynamoRecipeService, DynamoRecipeService>();
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            if ("development".Equals(env.EnvironmentName, StringComparison.OrdinalIgnoreCase))
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapHealthChecks("/health");
                }
            );
        }
    }
}
