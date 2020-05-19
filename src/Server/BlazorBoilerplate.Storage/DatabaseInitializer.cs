using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Server.Data.Core;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Storage.Core;
using Finbuckle.MultiTenant;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiLogItem = BlazorBoilerplate.Infrastructure.Storage.DataModels.ApiLogItem;
using UserProfile = BlazorBoilerplate.Infrastructure.Storage.DataModels.UserProfile;

namespace BlazorBoilerplate.Storage
{
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly PersistedGrantDbContext _persistedGrantContext;
        private readonly ConfigurationDbContext _configurationContext;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ILogger _logger;
        private readonly TenantStoreDbContext _tenantStoreDbContext;

        public DatabaseInitializer(
            ApplicationDbContext context,
            PersistedGrantDbContext persistedGrantContext,
            ConfigurationDbContext configurationContext,
            ILogger<DatabaseInitializer> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            TenantStoreDbContext tenantStoreDbContext)
        {
            _persistedGrantContext = persistedGrantContext;
            _configurationContext = configurationContext;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _tenantStoreDbContext = tenantStoreDbContext;
        }

        public virtual async Task SeedAsync()
        {
            //Apply EF Core migration
            await MigrateAsync();

            //Seed users and roles
            await SeedASPIdentityCoreAsync();

            //Seed clients and Api
            await SeedIdentityServerAsync();

            //Seed blazorboilerplate data
            await SeedBlazorBoilerplateAsync();
        }

        private async Task MigrateAsync()
        {
            await _tenantStoreDbContext.Database.MigrateAsync().ConfigureAwait(false);
            await _context.Database.MigrateAsync().ConfigureAwait(false);
            await _persistedGrantContext.Database.MigrateAsync().ConfigureAwait(false);
            await _configurationContext.Database.MigrateAsync().ConfigureAwait(false);
        }

        private async Task SeedASPIdentityCoreAsync()
        {
            const string adminRoleName = DefaultRoleNames.Administrator;
            const string userRoleName = DefaultRoleNames.User;

            if (!await _context.Users.AnyAsync())
            {
                //Generating inbuilt accounts
                await EnsureRoleAsync(adminRoleName, "Default administrator", ApplicationPermissions.GetAllPermissionValues());
                await EnsureRoleAsync(userRoleName, "Default user", new string[] { });

                await CreateUserAsync("admin", "admin123", "Admin", "Blazor", DefaultRoleNames.Administrator, "admin@blazoreboilerplate.com", "+1 (123) 456-7890", new string[] { adminRoleName });

                ApplicationUser user1 = await CreateUserAsync("user", "user123", DefaultRoleNames.User, "Blazor", "User Blazor", "user@blazoreboilerplate.com", "+1 (123) 456-7890", new string[] { userRoleName });
                ApplicationUser user2 = await CreateUserAsync("user2", "user123", DefaultRoleNames.User, "Blazor", "User Blazor", "user@blazoreboilerplate.com", "+1 (123) 456-7890", new string[] { userRoleName });

                var MicrosoftTenant = new TenantInfo("id-Microsoft", "Microsoft", "Microsoft Inc.", null, null);
                var ContosoTenant = new TenantInfo("id-Contoso", "Contoso", "Contoso Corp.", null, null);
                _tenantStoreDbContext.TenantInfo.Add(MicrosoftTenant);
                _tenantStoreDbContext.TenantInfo.Add(ContosoTenant);
                _tenantStoreDbContext.SaveChanges();

                await _userManager.AddClaimAsync(user1, new Claim("TenantId", MicrosoftTenant.Identifier));
                await _userManager.AddClaimAsync(user2, new Claim("TenantId", ContosoTenant.Identifier));

                _logger.LogInformation("Inbuilt account generation completed");
            }
            else
            {
                IdentityRole<Guid> adminRole = await _roleManager.FindByNameAsync(adminRoleName);
                var AllClaims = ApplicationPermissions.GetAllPermissionValues().Distinct();
                var RoleClaims = (await _roleManager.GetClaimsAsync(adminRole)).Select(c => c.Value).ToList();
                var NewClaims = AllClaims.Except(RoleClaims);
                foreach (string claim in NewClaims)
                {
                    await _roleManager.AddClaimAsync(adminRole, new Claim(ClaimConstants.Permission, claim));
                }
                var DeprecatedClaims = RoleClaims.Except(AllClaims);
                var roles = await _roleManager.Roles.ToListAsync();
                foreach (string claim in DeprecatedClaims)
                {
                    foreach (var role in roles)
                    {
                        await _roleManager.RemoveClaimAsync(role, new Claim(ClaimConstants.Permission, claim));
                    }
                }
            }
        }

        private async Task SeedBlazorBoilerplateAsync()
        {
            ApplicationUser user = await _userManager.FindByNameAsync("user");

            if (!_context.UserProfiles.Any())
            {
                UserProfile userProfile = new UserProfile
                {
                    UserId = user.Id,
                    ApplicationUser = user,
                    Count = 2,
                    IsNavOpen = true,
                    LastPageVisited = "/dashboard",
                    IsNavMinified = false,
                    LastUpdatedDate = DateTime.Now
                };
                _context.UserProfiles.Add(userProfile);
            }

            if (!_context.Todos.Any())
            {
                _context.Todos.AddRange(
                        new Todo
                        {
                            IsCompleted = false,
                            Title = "Test BlazorBoilerplate"
                        },
                        new Todo
                        {
                            IsCompleted = false,
                            Title = "Test BlazorBoilerplate 1",
                        }
                );
            }

            if (!_context.ApiLogs.Any())
            {
                _context.ApiLogs.AddRange(
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
            }

            _context.SaveChanges();
        }

        private async Task SeedIdentityServerAsync()
        {
            if (!await _configurationContext.Clients.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer Clients");
                foreach (var client in IdentityServerConfig.GetClients())
                {
                    _configurationContext.Clients.Add(client.ToEntity());
                }
                await _configurationContext.SaveChangesAsync();
            }
            if (!await _configurationContext.IdentityResources.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer Identity Resources");
                foreach (var resource in IdentityServerConfig.GetIdentityResources())
                {
                    _configurationContext.IdentityResources.Add(resource.ToEntity());
                }
                await _configurationContext.SaveChangesAsync();
            }
            if (!await _configurationContext.ApiResources.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer API Resources");
                foreach (var resource in IdentityServerConfig.GetApiResources())
                {
                    _configurationContext.ApiResources.Add(resource.ToEntity());
                }
                await _configurationContext.SaveChangesAsync();
            }
        }

        private async Task EnsureRoleAsync(string roleName, string description, string[] claims)
        {
            if ((await _roleManager.FindByNameAsync(roleName)) == null)
            {
                if (claims == null)
                    claims = new string[] { };

                string[] invalidClaims = claims.Where(c => ApplicationPermissions.GetPermissionByValue(c) == null).ToArray();
                if (invalidClaims.Any())
                    throw new Exception("The following claim types are invalid: " + string.Join(", ", invalidClaims));

                IdentityRole<Guid> applicationRole = new IdentityRole<Guid>(roleName);

                var result = await _roleManager.CreateAsync(applicationRole);

                IdentityRole<Guid> role = await _roleManager.FindByNameAsync(applicationRole.Name);

                foreach (string claim in claims.Distinct())
                {
                    result = await _roleManager.AddClaimAsync(role, new Claim(ClaimConstants.Permission, ApplicationPermissions.GetPermissionByValue(claim)));

                    if (!result.Succeeded)
                    {
                        await _roleManager.DeleteAsync(role);
                    }
                }
            }
        }

        private async Task<ApplicationUser> CreateUserAsync(string userName, string password, string firstName, string fullName, string lastName, string email, string phoneNumber, string[] roles)
        {
            var applicationUser = _userManager.FindByNameAsync(userName).Result;

            if (applicationUser == null)
            {
                applicationUser = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    FullName = fullName,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true
                };

                var result = _userManager.CreateAsync(applicationUser, password).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = _userManager.AddClaimsAsync(applicationUser, new Claim[]{
                        new Claim(JwtClaimTypes.Name, userName),
                        new Claim(JwtClaimTypes.GivenName, firstName),
                        new Claim(JwtClaimTypes.FamilyName, lastName),
                        new Claim(JwtClaimTypes.Email, email),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.PhoneNumber, phoneNumber)
                    }).Result;

                //add claims version of roles
                foreach (var role in roles.Distinct())
                {
                    await _userManager.AddClaimAsync(applicationUser, new Claim($"Is{role}", "true"));
                }

                ApplicationUser user = await _userManager.FindByNameAsync(applicationUser.UserName);

                try
                {
                    result = await _userManager.AddToRolesAsync(user, roles.Distinct());
                }
                catch
                {
                    await _userManager.DeleteAsync(user);
                    throw;
                }

                if (!result.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
            return applicationUser;
        }
    }
}