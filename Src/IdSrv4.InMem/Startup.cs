// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using IdentityServer4;
using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Twitter;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using IdentityServerAspNetIdentity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;

namespace IdSrv4.InMem
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            services.Configure<IISOptions>(options =>
            {
                options.AutomaticAuthentication = false;
                options.AuthenticationDisplayName = "Windows";
            });

            #region Identity Server 4 configuration
            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            });

            builder.AddAspNetIdentity<ApplicationUser>();

            // in-memory, code config
            //builder.AddInMemoryIdentityResources(Config.GetIdentityResources());
            //builder.AddInMemoryApiResources(Config.GetApis());
            //builder.AddInMemoryClients(Config.GetClients());

            // in-memory, json config
            builder.AddInMemoryIdentityResources(Configuration.GetSection("IdentityResources"));
            builder.AddInMemoryApiResources(Configuration.GetSection("ApiResources"));
            builder.AddInMemoryClients(Configuration.GetSection("clients"));

            #endregion

            #region Configure External Identity Providers
            ConfigureExternalIdentityProviders(services);
            #endregion

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            //services.AddAuthentication()
            //    .AddGoogle(options =>
            //    {
            //        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

            //        // register your IdentityServer with Google at https://console.developers.google.com
            //        // enable the Google+ API
            //        // set the redirect URI to http://localhost:5000/signin-google
            //        options.ClientId = "copy client ID from Google here";
            //        options.ClientSecret = "copy client secret from Google here";
            //    });
        }

        private void ConfigureExternalIdentityProviders(IServiceCollection services)
        {
            var autBuilder = services.AddAuthentication();

            // Google
            autBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, "Google Login", options => SetGoolgeOptions(options));

            // Facebook
            autBuilder.AddFacebook(FacebookDefaults.AuthenticationScheme, "Facebook Login", options => SetFacebookOptions(options));

            //Twitter
            autBuilder.AddTwitter(TwitterDefaults.AuthenticationScheme, "Twitter Login", options => SetTwitterOptions(options));

            //Azure AD
            autBuilder.AddAzureAd(options => Configuration.Bind("AzureAd", options));
        }

        private void SetTwitterOptions(TwitterOptions options)
        {
            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            options.ConsumerKey = "key";

            options.ConsumerSecret = "secret";
        }

        private void SetGoolgeOptions(GoogleOptions options)
        {
            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            options.ClientId = "386933463413-c94j09mgm1pi6pr5u9t24u3bhagqrtqq.apps.googleusercontent.com";
            options.ClientSecret = "M0KQP85Y9ALxNkmBbk5ObsIM";
        }

        private void SetFacebookOptions(FacebookOptions options)
        {
            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            options.AppId = "Appid";
            options.AppSecret = "appsecret";
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}