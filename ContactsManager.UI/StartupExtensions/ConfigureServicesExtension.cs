using ContactsManager.Core.Domain.IdentityEntities;
using ContactsManager.Core.ServiceContracts;
using CRUDExample.Filters.ActionFilters;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using Services;
namespace CRUDExample
{
    public static class ConfigureServicesExtension
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ResponseHeaderActionFilter>();

            //it adds controllers and views as services
            services.AddControllersWithViews(options =>
            {
                //options.Filters.Add<ResponseHeaderActionFilter>(5);

                var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();

                options.Filters.Add(new ResponseHeaderActionFilter(logger)
                {
                    Key = "My-Key-From-Global",
                    Value = "My-Value-From-Global",
                    Order = 2
                });
                // add "AutoValidateAntiforgeryTokenAttribute" to avoid XSRF attack 
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            //add services into IoC container
            services.AddScoped<ICountriesRepository, CountriesRepository>();
            services.AddScoped<IPersonsRepository, PersonsRepository>();

            services.AddScoped<ICountriesGetterService, CountriesGetterService>();
            services.AddScoped<ICountriesAdderService, CountriesAdderService>();
            services.AddScoped<ICountriesUploaderService, CountriesUploaderService>();

            services.AddScoped<IPersonsGetterService, PersonsGetterServiceWithFewExcelFields>();
            services.AddScoped<PersonsGetterService, PersonsGetterService>();

            services.AddScoped<IPersonsAdderService, PersonsAdderService>();
            services.AddScoped<IPersonsDeleterService, PersonsDeleterService>();
            services.AddScoped<IPersonsUpdaterService, PersonsUpdaterService>();
            services.AddScoped<IPersonsSorterService, PersonsSorterService>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddTransient<PersonsListActionFilter>();

            // Enabling Identity in this project
            services
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    // password complexity configure // like zod in frontend
                    options.Password.RequiredLength = 5;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireDigit = false;
                    options.Password.RequiredUniqueChars = 3;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
                .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

            //this peace of code block all the actions method for all the users
            //if you whant the user access to a url path with out login or signup then  use [AllowAnonymous] on the top of controller or action  method 
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                        .Build(); // enforce authorization policy(user must be authenticated) for all action methods
                // add a custom-policy
                options.AddPolicy("NotAuthorized", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        // this policy will return false is the user is authenticathed
                        return !context.User.Identity.IsAuthenticated;
                    });
                });

            });

            services.ConfigureApplicationCookie(options =>
            {
                // when the user is not permited for a special url then redirect him to this path
                options.LoginPath = "/Account/Login";
            });

            services.AddHttpLogging(options =>
            {
                options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
            });

            return services;
        }
    }
}
