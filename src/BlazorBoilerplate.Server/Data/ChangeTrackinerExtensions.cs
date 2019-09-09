using BlazorBoilerplate.Server.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Security.Claims;

namespace BlazorBoilerplate.Server.Data
{
    public static class ChangeTrackerExtensions
    {
        public static void SetShadowProperties(this ChangeTracker changeTracker, IUserSession userSession)
        {
            changeTracker.DetectChanges();
            Guid userId = Guid.Empty;
            var user = ClaimsPrincipal.Current;

            //TODO Fix this is not working, I cannot get the current User Id
            if (user != null)
            {
                var identity = user.Identity;
                if (identity != null)
                {
                    userId = user.Identity.IsAuthenticated ? new Guid(user.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value) : Guid.Empty;
                }
            }

            ApplicationDbContext dbContext = (ApplicationDbContext)changeTracker.Context;
            var timestamp = DateTime.UtcNow;

            foreach (var entry in changeTracker.Entries())
            {
                if (entry.Entity is IAuditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Property("CreatedOn").CurrentValue = timestamp;
                        entry.Property("CreatedBy").CurrentValue = userId;

                        //Fix usersession or add TenantId to Claims so we can store it in the future
                        //if (entry.Entity is ITenant)
                        //{
                        //    entry.Property("TenantId").CurrentValue = userSession.TenantId;
                        //}
                    }

                    if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified)
                    {
                        entry.Property("ModifiedOn").CurrentValue = timestamp;
                        entry.Property("ModifiedBy").CurrentValue = userId;
                    }
                }

                if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete)
                {
                    entry.State = EntityState.Modified;
                    entry.Property("IsDeleted").CurrentValue = true;
                }
            }
        }
    }
}
