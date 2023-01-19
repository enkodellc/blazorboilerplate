using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using BlazorBoilerplate.Shared.Localizer;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

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

        public virtual async Task Seed()
        {
            await Migrate();

            await ImportTranslations();

            await EnsureAdminIdentities();

            await SeedIdentityServer();

            _context.Database.ExecuteSqlRaw("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");
        }

        private async Task Migrate()
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

        private async Task SeedIdentityServer()
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

        public async Task EnsureAdminIdentities()
        {            
            foreach (var userFeature in Enum.GetValues<UserFeatures>())
                await EnsureRole(userFeature.ToString(), _entityPermissions.GetAllPermissionValuesFor(userFeature));

            await CreateUser(DefaultUserNames.Administrator, "admin123", "admin@blazorboilerplate.com", "+1 (123) 456-7890", new string[] { DefaultRoleNames.Administrator });
            
            _logger.LogInformation("Inbuilt account generation completed");
        }

        private async Task EnsureRole(string roleName, IEnumerable<string> claims)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                if (claims == null)
                    claims = Array.Empty<string>();

                string[] invalidClaims = claims.Where(c => _entityPermissions.GetPermissionByValue(c) == null).ToArray();
                if (invalidClaims.Any())
                    throw new Exception("The following claim types are invalid: " + string.Join(", ", invalidClaims));

                ApplicationRole applicationRole = new(roleName);

                var result = await _roleManager.CreateAsync(applicationRole);

                role = await _roleManager.FindByNameAsync(applicationRole.Name);

                foreach (string claim in claims.Distinct())
                {
                    result = await _roleManager.AddClaimAsync(role, new Claim(ApplicationClaimTypes.Permission, _entityPermissions.GetPermissionByValue(claim)));

                    if (!result.Succeeded)
                    {
                        await _roleManager.DeleteAsync(role);

                        throw new DomainException($"Unable to add claim {claim} to role {roleName}");
                    }
                }
            }

            var roleClaims = (await _roleManager.GetClaimsAsync(role)).Select(c => c.Value).ToList();
            var newClaims = claims.Except(roleClaims);

            foreach (string claim in newClaims)
                await _roleManager.AddClaimAsync(role, new Claim(ApplicationClaimTypes.Permission, claim));

            var deprecatedClaims = roleClaims.Except(claims);
            var roles = await _roleManager.Roles.ToListAsync();

            foreach (string claim in deprecatedClaims)
                foreach (var r in roles)
                    await _roleManager.RemoveClaimAsync(r, new Claim(ApplicationClaimTypes.Permission, claim));
        }
        private async Task<ApplicationUser> CreateUser(string userName, string password, string email, string phoneNumber, string[] roles = null)
        {
            var applicationUser = _userManager.FindByNameAsync(userName).Result;

            if (applicationUser == null)
            {
                applicationUser = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    EmailConfirmed = true
                };

                var result = _userManager.CreateAsync(applicationUser, password).Result;

                if (!result.Succeeded)
                    throw new Exception(result.Errors.First().Description);

                result = _userManager.AddClaimsAsync(applicationUser, new Claim[]{
                        new Claim(JwtClaimTypes.Name, userName),
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