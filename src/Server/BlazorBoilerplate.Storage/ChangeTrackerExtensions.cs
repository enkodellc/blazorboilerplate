using BlazorBoilerplate.Infrastructure.Storage.DataInterfaces;
using BlazorBoilerplate.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BlazorBoilerplate.Storage
{
    public static class ChangeTrackerExtensions
    {
        public static void SetShadowProperties(this ChangeTracker changeTracker, IUserSession userSession)
        {
            changeTracker.DetectChanges();
            Guid? userId = null;
            DateTime timestamp = DateTime.UtcNow;

            if (userSession.UserId != Guid.Empty)
                userId = userSession.UserId;

            foreach (EntityEntry entry in changeTracker.Entries().Where(e => e.State != EntityState.Unchanged))
            {
                if (entry.Entity is IAuditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Property("CreatedOn").CurrentValue = timestamp;

                        if (userId != null)
                            entry.Property("CreatedById").CurrentValue = userId;
                    }

                    if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified)
                    {
                        entry.Property("ModifiedOn").CurrentValue = timestamp;

                        if (userId != null)
                            entry.Property("ModifiedById").CurrentValue = userId;
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