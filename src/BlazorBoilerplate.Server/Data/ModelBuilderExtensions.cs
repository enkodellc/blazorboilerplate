using BlazorBoilerplate.Server.Data.Interfaces;
using BlazorBoilerplate.Server.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;

namespace BlazorBoilerplate.Server.Data
{
    //https://trailheadtechnology.com/entity-framework-core-2-1-automate-all-that-boring-boiler-plate/
    public static class ModelBuilderExtensions
    {
        public static void ShadowProperties(this ModelBuilder modelBuilder)
        {
            foreach (var tp in modelBuilder.Model.GetEntityTypes())
            {
                var t = tp.ClrType;

                // set auditing properties
                if (typeof(IAuditable).IsAssignableFrom(t))
                {
                    var method = SetAuditingShadowPropertiesMethodInfo.MakeGenericMethod(t);
                    method.Invoke(modelBuilder, new object[] { modelBuilder });
                }

                // set tenant properties
                if (typeof(ITenant).IsAssignableFrom(t))
                {
                    var method = SetTenantShadowPropertyMethodInfo.MakeGenericMethod(t);
                    method.Invoke(modelBuilder, new object[] { modelBuilder });
                }

                // set soft delete property
                if (typeof(ISoftDelete).IsAssignableFrom(t))
                {
                    var method = SetIsDeletedShadowPropertyMethodInfo.MakeGenericMethod(t);
                    method.Invoke(modelBuilder, new object[] { modelBuilder });
                }
            }
        }

        private static readonly MethodInfo SetIsDeletedShadowPropertyMethodInfo = typeof(ModelBuilderExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(t => t.IsGenericMethod && t.Name == "SetIsDeletedShadowProperty");

        private static readonly MethodInfo SetTenantShadowPropertyMethodInfo = typeof(ModelBuilderExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(t => t.IsGenericMethod && t.Name == "SetTenantShadowProperty");

        private static readonly MethodInfo SetAuditingShadowPropertiesMethodInfo = typeof(ModelBuilderExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(t => t.IsGenericMethod && t.Name == "SetAuditingShadowProperties");

        public static void SetIsDeletedShadowProperty<T>(ModelBuilder builder) where T : class, ISoftDelete
        {
            // define shadow property
            builder.Entity<T>().Property<bool>("IsDeleted");
        }

        public static void SetTenantShadowProperty<T>(ModelBuilder builder) where T : class, ITenant
        {
            // define shadow property
            builder.Entity<T>().Property<int>("TenantId");
            // define FK to Tenant
            builder.Entity<T>().HasOne<Tenant>().WithMany().HasForeignKey("TenantId").OnDelete(DeleteBehavior.Restrict);
        }

        public static void SetAuditingShadowProperties<T>(ModelBuilder builder) where T : class, IAuditable
        {
            // define shadow properties
            builder.Entity<T>().Property<DateTime>("CreatedOn").HasDefaultValueSql("GetUtcDate()");
            builder.Entity<T>().Property<DateTime>("ModifiedOn").HasDefaultValueSql("GetUtcDate()");
            builder.Entity<T>().Property<Guid>("CreatedBy");
            builder.Entity<T>().Property<Guid>("ModifiedBy");

            // define FKs to User
            //builder.Entity<T>().HasOne<ApplicationUser>().WithMany().HasForeignKey("CreatedBy").OnDelete(DeleteBehavior.Restrict);
            //builder.Entity<T>().HasOne<ApplicationUser>().WithMany().HasForeignKey("ModifiedBy").OnDelete(DeleteBehavior.Restrict);
        }
    }
}
