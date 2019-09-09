using BlazorBoilerplate.Server.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;

namespace BlazorBoilerplate.Server.Data
{
    public interface IApplicationDbContextSeed
    { 
        void SeedDb();
    }

    //https://stackoverflow.com/questions/34343599/how-to-seed-users-and-roles-with-code-first-migration-using-identity-asp-net-cor?rq=1
    public class ApplicationDbContextSeed : IApplicationDbContextSeed
    {
        private ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public ApplicationDbContextSeed(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void SeedDb()
        {
            // https://gooroo.io/GoorooTHINK/Article/17333/Custom-user-roles-and-rolebased-authorization-in-ASPNET-core/32835
            string[] roleNames = { "SuperAdmin", "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                //creating the roles and seeding them to the database
                if (!_dbContext.Roles.Any(r => r.Name == roleName))
                {
                    _roleManager.CreateAsync(new IdentityRole<Guid>(roleName)).Wait();
                }
                else
                {
                    return; //If we have roles then database has been seeded already
                }
            }   

            ApplicationUser user = new ApplicationUser
            {
                Id = new Guid("09C0D2E2-B003-4BE8-A62A-08D7268AF58E"),
                UserName = "user",
                NormalizedUserName = "USER",
                Email = "support@blazorboilerplate.com",
                NormalizedEmail = "SUPPORT@BLAZORBOILERPLATE.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAEAACcQAAAAECiXBZr80VbgUNsRrJtpqG0jlI32isbBEQZbIO2iyZlux/0IqqRkhHNnSP3sPGkfSQ==",
                SecurityStamp = "HWTICTXU2EZWPX4CYA54JNU37DBZUQYJ",
                ConcurrencyStamp = "ece56175-e479-404f-956e-13edbd9a9b75",
                LockoutEnabled = true,
                AccessFailedCount = 0,
                FirstName = "Blazor",
                LastName = "Boilerplate"
            };

            _userManager.CreateAsync(user).Wait();
            _userManager.AddToRoleAsync(user, "Admin").Wait();


            UserProfile userProfile = new UserProfile
            {
               UserId = new Guid("09C0D2E2-B003-4BE8-A62A-08D7268AF58E"),
               Count = 2,
               IsNavOpen = true,
               LastPageVisited = "/dashboard",
               IsNavMinified = false,
               LastUpdatedDate = DateTime.Now
            };
            _dbContext.UserProfiles.Add(userProfile);

            _dbContext.Todos.AddRange(
                    new Todo
                    {
                        IsCompleted = false,
                        Title = "Test Blazor Boilerplate"
                    },
                    new Todo
                    {
                        IsCompleted = false,
                        Title = "Test Blazor Boilerplate 1",
                    }
                );

            _dbContext.ApiLogs.AddRange(
                new ApiLogItem
                {
                    RequestTime = DateTime.Now,
                    ResponseMillis = 30,
                    StatusCode = 200,
                    Method = "Get",
                    Path = "/api/seed",
                    QueryString = "",
                    RequestBody = "",
                    ResponseBody = "",
                    IPAddress = "::1"
                },
                new ApiLogItem
                {
                    RequestTime = DateTime.Now,
                    ResponseMillis = 30,
                    StatusCode = 200,
                    Method = "Get",
                    Path = "/api/seed",
                    QueryString = "",
                    RequestBody = "",
                    ResponseBody = "",
                    IPAddress = "::1",
                    ApplicationUserId = user.Id
                }
            );

            _dbContext.SaveChanges();
        }
    }
}
