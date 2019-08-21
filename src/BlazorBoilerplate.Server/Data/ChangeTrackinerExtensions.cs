using BlazorBoilerplate.Server.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace BlazorBoilerplate.Server.Data
{
    public static class ChangeTrackerExtensions
    {
        public static void SetShadowProperties(this ChangeTracker changeTracker, IUserSession userSession)
        {
            changeTracker.DetectChanges();

            var timestamp = DateTime.UtcNow;

            foreach (var entry in changeTracker.Entries())
            {
                if (entry.Entity is IAuditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Property("CreatedOn").CurrentValue = timestamp;
                        entry.Property("CreatedBy").CurrentValue = userSession.UserId;
                        if (entry.Entity is ITenant)
                        {
                            entry.Property("TenantId").CurrentValue = userSession.TenantId;
                        }
                    }

                    if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified)
                    {
                        entry.Property("ModifiedOn").CurrentValue = timestamp;
                        entry.Property("ModifiedBy").CurrentValue = userSession.UserId;
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
