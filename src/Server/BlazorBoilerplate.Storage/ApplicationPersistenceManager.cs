using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Localization;
using Breeze.Persistence;
using Breeze.Persistence.EFCore;
using Finbuckle.MultiTenant;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using EntityState = Breeze.Persistence.EntityState;

namespace BlazorBoilerplate.Storage
{
    public class ApplicationPersistenceManager : EFPersistenceManager<ApplicationDbContext>
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IStringLocalizer<Strings> L;
        public ApplicationPersistenceManager(ApplicationDbContext dbContext,
            IHttpContextAccessor accessor,
            IStringLocalizer<Strings> l) : base(dbContext)
        {
            httpContextAccessor = accessor;
            L = l;
        }
        public DbSet<TEntity> GetEntities<TEntity>() where TEntity : class
        {
            var requiredPermissions = typeof(TEntity).GetCustomAttribute<PermissionsAttribute>(false);

            if (requiredPermissions != null && (requiredPermissions.Actions & Actions.Read) == Actions.Read)
            {
                var user = httpContextAccessor?.HttpContext?.User;

                if (user == null || user.Identity.IsAuthenticated == false)
                    throw new UnauthorizedAccessException(L["AuthenticationRequired"] + ": " + L["LoginRequired"]);
                else if (!user.Claims.Any(c => c.Type == "permission" && c.Value == $"{typeof(TEntity).Name}.{Actions.Read}"))
                    throw new UnauthorizedAccessException(L["Operation not allowed"] + ": " + L["NotAuthorizedTo"]);
            }

            return Context.Set<TEntity>();
        }

        protected override Dictionary<Type, List<EntityInfo>> BeforeSaveEntities(Dictionary<Type, List<EntityInfo>> saveMap)
        {
            var errors = new List<EFEntityError>();
            var user = httpContextAccessor?.HttpContext?.User;

            foreach (var type in saveMap.Keys)
            {
                var requiredPermissions = type.GetCustomAttribute<PermissionsAttribute>(false);

                if (requiredPermissions != null)
                {
                    Actions? requiredAction = null;

                    foreach (var entityInfo in saveMap[type])
                    {
                        switch (entityInfo.EntityState)
                        {
                            case EntityState.Added:
                                requiredAction = Actions.Create;
                                break;
                            case EntityState.Modified:
                                requiredAction = Actions.Update;
                                break;
                            case EntityState.Deleted:
                                requiredAction = Actions.Delete;
                                break;
                        }

                        if ((requiredPermissions.Actions & requiredAction) == requiredAction)
                        {
                            if (user == null || user.Identity.IsAuthenticated == false)
                                errors.Add(new EFEntityError(entityInfo, L["AuthenticationRequired"], L["LoginRequired"], null));
                            else if (!user.Claims.Any(c => c.Type == "permission" && c.Value == $"{entityInfo.Entity.GetType().Name}.{requiredAction}"))
                                errors.Add(new EFEntityError(entityInfo, L["Operation not allowed"], L["NotAuthorizedTo"], null));
                        }
                    }
                }
            }

            if (errors.Count > 0)
            {
                if (user == null || user.Identity.IsAuthenticated == false)
                    throw new EntityErrorsException(errors) { StatusCode = HttpStatusCode.Unauthorized };
                else
                    throw new EntityErrorsException(errors) { StatusCode = HttpStatusCode.Forbidden };
            }

            return saveMap;
        }

        protected override bool BeforeSaveEntity(EntityInfo entityInfo)
        {
            if (entityInfo.Entity is UserProfile userProfile)
                userProfile.LastUpdatedDate = DateTime.Now;

            return true;
        }

        public async Task<UserProfile> GetUserProfile()
        {
            var user = httpContextAccessor.HttpContext.User;
            var userProfile = await Context.UserProfiles.SingleOrDefaultAsync(i => i.ApplicationUser.NormalizedUserName == user.Identity.Name.ToUpper());

            if (userProfile == null)
            {
                userProfile = new UserProfile
                {
                    TenantId = httpContextAccessor.HttpContext.GetMultiTenantContext().TenantInfo.Id,
                    UserId = new Guid(user.Claims.Single(c => c.Type == JwtClaimTypes.Subject).Value),
                    LastUpdatedDate = DateTime.Now
                };

                await Context.UserProfiles.Upsert(userProfile).On(u => new { u.TenantId, u.UserId }).RunAsync();
                //see https://github.com/artiomchi/FlexLabs.Upsert/issues/29
                userProfile = await Context.UserProfiles.SingleOrDefaultAsync(i => i.UserId == userProfile.UserId);
            }

            return userProfile;
        }
    }
}
