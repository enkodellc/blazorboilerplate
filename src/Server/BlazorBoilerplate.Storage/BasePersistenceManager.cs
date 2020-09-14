using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.SqlLocalizer;
using Breeze.Persistence;
using Breeze.Persistence.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using EntityState = Breeze.Persistence.EntityState;

namespace BlazorBoilerplate.Storage
{
    public abstract class BasePersistenceManager<T> : EFPersistenceManager<T> where T : DbContext
    {
        protected readonly IHttpContextAccessor httpContextAccessor;
        protected readonly IStringLocalizer<Global> L;
        public BasePersistenceManager(T dbContext,
            IHttpContextAccessor accessor,
            IStringLocalizer<Global> l) : base(dbContext)
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
                else if (!user.Claims.Any(c => c.Type == ClaimConstants.Permission && c.Value == $"{typeof(TEntity).Name}.{Actions.Read}"))
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
                            else if (!user.Claims.Any(c => c.Type == ClaimConstants.Permission && c.Value == $"{entityInfo.Entity.GetType().Name}.{requiredAction}"))
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
    }
}
