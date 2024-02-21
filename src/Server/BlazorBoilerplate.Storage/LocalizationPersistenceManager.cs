﻿using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.Localizer;
using Breeze.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Storage
{
    public class LocalizationPersistenceManager : BasePersistenceManager<LocalizationDbContext>
    {
        public LocalizationPersistenceManager(LocalizationDbContext dbContext,
            IHttpContextAccessor accessor,
            IServiceProvider serviceProvider,
            IStringLocalizer<Global> l) : base(dbContext, accessor, serviceProvider, l)
        { }

        protected override bool BeforeSaveEntity(EntityInfo entityInfo)
        {
            if (entityInfo.EntityState == EntityState.Added || entityInfo.EntityState == EntityState.Modified)
                if (entityInfo.Entity is LocalizationRecord localizationRecord)
                {
                    if (string.IsNullOrEmpty(localizationRecord.MsgId))
                        return false;

                    if (string.IsNullOrWhiteSpace(localizationRecord.MsgIdPlural))
                        localizationRecord.MsgIdPlural = null;
                }

            return base.BeforeSaveEntity(entityInfo);
        }
    }
}
