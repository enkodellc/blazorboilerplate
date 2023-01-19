using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.Localizer;
using Breeze.Persistence;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BlazorBoilerplate.Storage
{
    public class ApplicationPersistenceManager : BasePersistenceManager<ApplicationDbContext>
    {
        public ApplicationPersistenceManager(ApplicationDbContext dbContext,
            IHttpContextAccessor accessor,
            IServiceProvider serviceProvider,
            ILogger<ApplicationPersistenceManager> logger,
            IStringLocalizer<Global> l) : base(dbContext, accessor, serviceProvider, logger, l)
        { }

        protected override bool BeforeSaveEntity(EntityInfo entityInfo)
        {
            if (entityInfo.Entity is UserProfile userProfile)
                userProfile.LastUpdatedDate = DateTime.Now;
            else if (entityInfo.Entity is ApplicationUser applicationUser && entityInfo.EntityState == Breeze.Persistence.EntityState.Modified)
            {
                var props = DbContext.Entry(applicationUser).GetDatabaseValues();
                applicationUser.PasswordHash = props.GetValue<string>("PasswordHash");
                applicationUser.SecurityStamp = props.GetValue<string>("SecurityStamp");
            }

            return true;
        }
        public async Task<UserProfile> GetUserProfile()
        {
            var user = httpContextAccessor.HttpContext.User;

            if (user.Identity.Name == null)
                throw new Exception("user.Identity.Name is null");

            var userProfile = await Context.UserProfiles.SingleOrDefaultAsync(i => i.ApplicationUser.NormalizedUserName == user.Identity.Name.ToUpper());

            if (userProfile == null)
            {
                var userId = new Guid(user.GetSubjectId());

                userProfile = await Context.UserProfiles.SingleOrDefaultAsync(i => i.UserId == userId);

                if (userProfile == null)
                {
                    userProfile = new UserProfile { UserId = userId };

                    Context.UserProfiles.Add(userProfile);
                }

                userProfile.LastUpdatedDate = DateTime.Now;

                await Context.SaveChangesAsync();
            }

            return userProfile;
        }
    }
}
