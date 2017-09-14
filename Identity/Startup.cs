using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IdentityServer4.Models;
using IdentityServer4;
using IdentityServer4.Test;
using System.Security.Claims;

namespace Identity
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddIdentityServer()
               .AddTemporarySigningCredential()
               .AddInMemoryIdentityResources(new List<IdentityResource> {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                    new IdentityResources.Email()
               })
               .AddInMemoryApiResources(new[] {
                    new ApiResource("my-api", "My Api")
               })
               .AddInMemoryClients(new[] {
                    new Client
                    {
                        ClientId = "my-client",
                        ClientName = "My Client",
                        AllowedGrantTypes = GrantTypes.Implicit,
                        AllowAccessTokensViaBrowser = true,
                        AllowedCorsOrigins = { "http://localhost:4200" }, // My Client is a Angular application served on port 4200
                        AllowRememberConsent = true,
                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.Email,
                            "my-api"
                        },
                        RedirectUris = { "http://localhost:4200/callback.html" },
                        PostLogoutRedirectUris = { "http://localhost:4200/index.html" }
                    }
               })
               .AddTestUsers(new List<TestUser> {
                   new TestUser {
                       SubjectId = "alice",
                       Username = "Alice",
                       Password = "12345",
                       Claims = {
                           new Claim(IdentityServerConstants.StandardScopes.Email, "alice@gmail.com"),
                           new Claim(IdentityServerConstants.StandardScopes.Address, "21 Jump Street")
                       }
                   }
               });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory
                .AddConsole(Configuration.GetSection("Logging"))
                .AddDebug();

            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
