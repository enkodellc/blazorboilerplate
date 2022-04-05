using BlazorBoilerplate.Infrastructure.Storage.DataInterfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BlazorBoilerplate.Storage
{
    //https://trailheadtechnology.com/entity-framework-core-2-1-automate-all-that-boring-boiler-plate/
    public static class ModelBuilderExtensions
    {
        public static void ShadowProperties(this ModelBuilder modelBuilder)
        {
            foreach (var tp in modelBuilder.Model.GetEntityTypes())
            {
                Type t = tp.ClrType;

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

        public static void SetIsDeletedShadowProperty<T>(ModelBuilder builder) where T : class, ISoftDelete
        {
            // define shadow property
            builder.Entity<T>().Property<bool>("IsDeleted");
        }
    }
}