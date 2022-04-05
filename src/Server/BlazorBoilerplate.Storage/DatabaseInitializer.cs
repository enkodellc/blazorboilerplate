using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using BlazorBoilerplate.Shared.Localizer;
using Finbuckle.MultiTenant;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ApiLogItem = BlazorBoilerplate.Infrastructure.Storage.DataModels.ApiLogItem;
using UserProfile = BlazorBoilerplate.Infrastructure.Storage.DataModels.UserProfile;

namespace BlazorBoilerplate.Storage
{
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly LocalizationDbContext _localizationDbContext;
        private readonly PersistedGrantDbContext _persistedGrantContext;
        private readonly ConfigurationDbContext _configurationContext;
        private readonly ApplicationDbContext _context;
        private readonly TenantStoreDbContext _tenantStoreDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly EntityPermissions _entityPermissions;
        private readonly ILocalizationProvider _localizationProvider;
        private readonly ILogger _logger;

        public DatabaseInitializer(
            TenantStoreDbContext tenantStoreDbContext,
            LocalizationDbContext localizationDbContext,
            ApplicationDbContext context,
            PersistedGrantDbContext persistedGrantContext,
            ConfigurationDbContext configurationContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            EntityPermissions entityPermissions,
            ILocalizationProvider localizationProvider,
            ILogger<DatabaseInitializer> logger)
        {
            _tenantStoreDbContext = tenantStoreDbContext;
            _localizationDbContext = localizationDbContext;
            _persistedGrantContext = persistedGrantContext;
            _configurationContext = configurationContext;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _entityPermissions = entityPermissions;
            _localizationProvider = localizationProvider;
            _logger = logger;
        }

        public virtual async Task SeedAsync()
        {
            //Apply EF Core migration
            await MigrateAsync();

            await ImportTranslations();

            await EnsureAdminIdentitiesAsync();

            await SeedIdentityServerAsync();

            //Seed blazorboilerplate sample data
            await SeedDemoDataAsync();
        }

        private async Task MigrateAsync()
        {
            await _tenantStoreDbContext.Database.MigrateAsync();
            await _localizationDbContext.Database.MigrateAsync();
            await _context.Database.MigrateAsync();
            await _persistedGrantContext.Database.MigrateAsync();
            await _configurationContext.Database.MigrateAsync();
        }

        private async Task ImportTranslations()
        {
            try
            {
                if (!await _localizationDbContext.LocalizationRecords.AnyAsync())
                    await ((StorageLocalizationProvider)_localizationProvider).InitDbFromPoFiles(_localizationDbContext);
            }
            catch (Exception ex)
            {
                _logger.LogError("Importing PO files in db error: {0}", ex.GetBaseException().Message);
            }
        }

        private async Task SeedDemoDataAsync()
        {
            if ((await _userManager.FindByNameAsync(DefaultUserNames.User)) == null)
            {
                await CreateUserAsync(DefaultUserNames.User, "user123", "User", "Blazor", "user@blazorboilerplate.com", "+1 (123) 456-7890");
            }

            if (_tenantStoreDbContext.TenantInfo.Count() < 2)
            {
                _tenantStoreDbContext.TenantInfo.Add(new TenantInfo() { Id = "tenant1", Identifier = "tenant1.local", Name = "Microsoft Inc." });
                _tenantStoreDbContext.TenantInfo.Add(new TenantInfo() { Id = "tenant2", Identifier = "tenant2.local", Name = "Contoso Corp." });

                _tenantStoreDbContext.SaveChanges();
            }

            ApplicationUser user = await _userManager.FindByNameAsync(DefaultUserNames.User);

            if (!_context.UserProfiles.Any())
                _context.UserProfiles.Add(new UserProfile
                {
                    UserId = user.Id,
                    ApplicationUser = user,
                    Count = 2,
                    IsNavOpen = true,
                    LastPageVisited = "/dashboard",
                    IsNavMinified = false,
                    LastUpdatedDate = DateTime.Now
                });

            if (!_context.Todos.Any())
            {
                var rnd = new Random();

                var fruits = new string[] { "apples", "pears", "peaches", "oranges" };

                var users = _context.Users.ToArray();

                for (int i = 0; i < 1000; i++)
                    _context.Todos.Add(
                            new Todo
                            {
                                IsCompleted = false,
                                Title = $"Buy {rnd.Next(2, 5)} {fruits[rnd.Next(fruits.Length)]}",
                                CreatedById = users[rnd.Next(users.Length)].Id
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
            if (!await _configurationContext.ApiScopes.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer API Scopes");
                foreach (var scope in IdentityServerConfig.GetApiScopes)
                    _configurationContext.ApiScopes.Add(scope.ToEntity());

                await _configurationContext.SaveChangesAsync();
            }

            if (!await _configurationContext.Clients.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer Clients");
                foreach (var client in IdentityServerConfig.GetClients)
                    _configurationContext.Clients.Add(client.ToEntity());

                await _configurationContext.SaveChangesAsync();
            }

            if (!await _configurationContext.IdentityResources.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer Identity Resources");
                foreach (var resource in IdentityServerConfig.GetIdentityResources)
                    _configurationContext.IdentityResources.Add(resource.ToEntity());

                await _configurationContext.SaveChangesAsync();
            }

            if (!await _configurationContext.ApiResources.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer API Resources");
                foreach (var resource in IdentityServerConfig.GetApiResources)
                    _configurationContext.ApiResources.Add(resource.ToEntity());

                await _configurationContext.SaveChangesAsync();
            }
        }

        public async Task EnsureAdminIdentitiesAsync()
        {
            await EnsureRoleAsync(DefaultRoleNames.Administrator, _entityPermissions.GetAllPermissionValues());
            await CreateUserAsync(DefaultUserNames.Administrator, "admin123", "Admin", "Blazor", "admin@blazorboilerplate.com", "+1 (123) 456-7890", new string[] { DefaultRoleNames.Administrator });

            ApplicationRole adminRole = await _roleManager.FindByNameAsync(DefaultRoleNames.Administrator);
            var AllClaims = _entityPermissions.GetAllPermissionValues().Distinct();
            var RoleClaims = (await _roleManager.GetClaimsAsync(adminRole)).Select(c => c.Value).ToList();
            var NewClaims = AllClaims.Except(RoleClaims);

            foreach (string claim in NewClaims)
                await _roleManager.AddClaimAsync(adminRole, new Claim(ApplicationClaimTypes.Permission, claim));

            var DeprecatedClaims = RoleClaims.Except(AllClaims);
            var roles = await _roleManager.Roles.ToListAsync();

            foreach (string claim in DeprecatedClaims)
                foreach (var role in roles)
                    await _roleManager.RemoveClaimAsync(role, new Claim(ApplicationClaimTypes.Permission, claim));

            _logger.LogInformation("Inbuilt account generation completed");
        }

        private async Task EnsureRoleAsync(string roleName, string[] claims)
        {
            if ((await _roleManager.FindByNameAsync(roleName)) == null)
            {
                if (claims == null)
                    claims = Array.Empty<string>();

                string[] invalidClaims = claims.Where(c => _entityPermissions.GetPermissionByValue(c) == null).ToArray();
                if (invalidClaims.Any())
                    throw new Exception("The following claim types are invalid: " + string.Join(", ", invalidClaims));

                ApplicationRole applicationRole = new(roleName);

                var result = await _roleManager.CreateAsync(applicationRole);

                ApplicationRole role = await _roleManager.FindByNameAsync(applicationRole.Name);

                foreach (string claim in claims.Distinct())
                {
                    result = await _roleManager.AddClaimAsync(role, new Claim(ApplicationClaimTypes.Permission, _entityPermissions.GetPermissionByValue(claim)));

                    if (!result.Succeeded)
                        await _roleManager.DeleteAsync(role);
                }
            }
        }

        private async Task<ApplicationUser> CreateUserAsync(string userName, string password, string firstName, string lastName, string email, string phoneNumber, string[] roles = null)
        {
            var applicationUser = _userManager.FindByNameAsync(userName).Result;

            if (applicationUser == null)
            {
                applicationUser = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true
                };

                var result = _userManager.CreateAsync(applicationUser, password).Result;

                if (!result.Succeeded)
                    throw new Exception(result.Errors.First().Description);

                result = _userManager.AddClaimsAsync(applicationUser, new Claim[]{
                        new Claim(JwtClaimTypes.Name, userName),
                        new Claim(JwtClaimTypes.GivenName, firstName),
                        new Claim(JwtClaimTypes.FamilyName, lastName),
                        new Claim(JwtClaimTypes.Email, email),
                        new Claim(JwtClaimTypes.EmailVerified, ClaimValues.trueString, ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.PhoneNumber, phoneNumber)
                    }).Result;

                if (!result.Succeeded)
                    throw new Exception(result.Errors.First().Description);

                //add claims version of roles
                if (roles != null)
                {
                    foreach (var role in roles.Distinct())
                    {
                        await _userManager.AddClaimAsync(applicationUser, new Claim($"Is{role}", ClaimValues.trueString));
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
            }

            return applicationUser;
        }
    }
}