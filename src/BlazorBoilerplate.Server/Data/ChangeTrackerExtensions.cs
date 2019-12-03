using BlazorBoilerplate.Server.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Security.Claims;

namespace BlazorBoilerplate.Server.Data
{
    public static class ChangeTrackerExtensions
    {
        public static void SetShadowProperties(this ChangeTracker changeTracker, IUserSession userSession)
        {
            changeTracker.DetectChanges();
            ApplicationDbContext dbContext = (ApplicationDbContext)changeTracker.Context;
            Guid userId = Guid.Empty;
            var timestamp = DateTime.UtcNow;

            if (userSession.UserId != Guid.Empty)
            {
                userId = userSession.UserId;
            }

            foreach (var entry in changeTracker.Entries())
            {
                //Auditable Entity Model
                if (entry.Entity is IAuditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Property("CreatedOn").CurrentValue = timestamp;
                        entry.Property("CreatedBy").CurrentValue = userId;

                        //Add TenantId to Claims so we can store it in the future
                        if (entry.Entity is ITenant)
                        {
                            entry.Property("TenantId").CurrentValue = userSession.TenantId;
                        }
                    }

                    if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified)
                    {
                        entry.Property("ModifiedOn").CurrentValue = timestamp;
                        entry.Property("ModifiedBy").CurrentValue = userId;
                    }
                }

                //Soft Delete Entity Model
                if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete)
                {
                    entry.State = EntityState.Modified;
                    entry.Property("IsDeleted").CurrentValue = true;
                }
            }
        }
    }
}
