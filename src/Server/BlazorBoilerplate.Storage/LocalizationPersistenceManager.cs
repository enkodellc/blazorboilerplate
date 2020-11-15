using BlazorBoilerplate.Shared.Localizer;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Storage
{
    public class LocalizationPersistenceManager : BasePersistenceManager<LocalizationDbContext>
    {
        public LocalizationPersistenceManager(LocalizationDbContext dbContext,
            IHttpContextAccessor accessor,
            IValidatorFactory factory,
            IStringLocalizer<Global> l) : base(dbContext, accessor, factory, l)
        { }
    }
}
