using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using BlazorBoilerplate.Shared.Localizer;
using Breeze.Persistence;
using Breeze.Persistence.EFCore;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Reflection;
using EntityState = Breeze.Persistence.EntityState;

namespace BlazorBoilerplate.Storage
{
    public abstract class BasePersistenceManager<T> : EFPersistenceManager<T> where T : DbContext
    {
        protected readonly IHttpContextAccessor httpContextAccessor;
        protected readonly IValidatorFactory validatorFactory;
        protected readonly IStringLocalizer<Global> L;
        public BasePersistenceManager(T dbContext,
            IHttpContextAccessor accessor,
            IValidatorFactory factory,
            IStringLocalizer<Global> l) : base(dbContext)
        {
            httpContextAccessor = accessor;
            validatorFactory = factory;
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
                else if (!user.Claims.Any(c => c.Type == ApplicationClaimTypes.Permission && c.Value == $"{typeof(TEntity).Name}.{Actions.Read}"))
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

                        var entityType = entityInfo.Entity.GetType();

                        if ((requiredPermissions.Actions & requiredAction) == requiredAction)
                        {
                            if (user == null || user.Identity.IsAuthenticated == false)
                                errors.Add(new EFEntityError(entityInfo, L["AuthenticationRequired"], L["LoginRequired"], null));
                            else if (!user.Claims.Any(c => c.Type == ApplicationClaimTypes.Permission && c.Value == $"{entityType.Name}.{requiredAction}"))
                                errors.Add(new EFEntityError(entityInfo, L["Operation not allowed"], L["NotAuthorizedTo"], null));
                        }

                        if (entityInfo.EntityState == EntityState.Added || entityInfo.EntityState == EntityState.Modified)
                        {
                            var validator = validatorFactory.GetValidator(entityType);

                            if (validator == null)
                            {
                                var iface = entityType.GetInterfaces().SingleOrDefault(i => i.Namespace == typeof(LocalizationRecord).Namespace);

                                if (iface != null)
                                {
                                    validator = validatorFactory.GetValidator(iface);

                                    if (validator != null)
                                    {
                                        var results = validator.Validate(new ValidationContext<object>(entityInfo.Entity));

                                        if (!results.IsValid)
                                            errors.AddRange(results.Errors.Select(i => new EFEntityError(entityInfo, i.ErrorCode, i.ErrorMessage, i.PropertyName)));
                                    }
                                }
                            }
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
