using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.Dto.Db.Validators;
using BlazorBoilerplate.Shared.SqlLocalizer;
using Breeze.Persistence;
using Breeze.Persistence.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorBoilerplate.Storage
{
    public class LocalizationPersistenceManager : BasePersistenceManager<LocalizationDbContext>
    {
        private LocalizationRecordValidator validator;
        public LocalizationPersistenceManager(LocalizationDbContext dbContext,
            IHttpContextAccessor accessor,
            IStringLocalizer<Global> l) : base(dbContext, accessor, l)
        {
            validator = new LocalizationRecordValidator(L);
        }

        protected override Dictionary<Type, List<EntityInfo>> BeforeSaveEntities(Dictionary<Type, List<EntityInfo>> saveMap)
        {
            saveMap = base.BeforeSaveEntities(saveMap);

            if (saveMap.TryGetValue(typeof(LocalizationRecord), out List<EntityInfo> localizationRecords))
            {
                foreach (var entityInfo in localizationRecords.Where(i => i.EntityState == EntityState.Modified || i.EntityState == EntityState.Added))
                {
                    var results = validator.Validate((Shared.Dto.Db.ILocalizationRecord)entityInfo.Entity);

                    if (!results.IsValid)
                    {
                        var errors = results.Errors.Select(i =>
                        {
                            return new EFEntityError(entityInfo, i.ErrorCode, i.ErrorMessage, i.PropertyName);
                        });

                        throw new EntityErrorsException(errors);
                    }
                }
            }

            return saveMap;
        }
    }
}
