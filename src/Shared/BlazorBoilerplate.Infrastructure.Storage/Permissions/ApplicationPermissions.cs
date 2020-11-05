using BlazorBoilerplate.Constants;
using Finbuckle.MultiTenant;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace BlazorBoilerplate.Infrastructure.Storage.Permissions
{
    public class ApplicationPermissions
    {
        private static readonly ReadOnlyCollection<ApplicationPermission> AllPermissions;
        private static readonly ReadOnlyCollection<ApplicationPermission> AllPermissionsForTenants;

        private bool IsMasterTenant;
        /// <summary>
        /// Generates ApplicationPermissions based on Permissions Type by iterating over its nested classes and getting constant strings in each class as Value and Name, LocalizedDescriptionAttribute of the constant string as Description, the nested class name as GroupName.
        /// </summary>
        static ApplicationPermissions()
        {
            List<ApplicationPermission> allPermissions = new List<ApplicationPermission>();
            IEnumerable<object> permissionClasses = typeof(Permissions).GetNestedTypes(BindingFlags.Static | BindingFlags.Public).Cast<TypeInfo>();
            foreach (TypeInfo permissionClass in permissionClasses)
            {
                IEnumerable<FieldInfo> permissions = permissionClass.DeclaredFields.Where(f => f.IsLiteral);
                foreach (FieldInfo permission in permissions)
                {
                    ApplicationPermission applicationPermission = new ApplicationPermission
                    {
                        Value = permission.GetValue(null).ToString(),
                        Name = permission.GetValue(null).ToString().Replace('.', ' '),
                        GroupName = permissionClass.Name
                    };

                    DisplayAttribute[] attributes = (DisplayAttribute[])permission.GetCustomAttributes(typeof(DisplayAttribute), false);

                    applicationPermission.Description = attributes != null && attributes.Length > 0 ? attributes[0].Description : applicationPermission.Name;

                    allPermissions.Add(applicationPermission);
                }
            }

            var entitiesWithPermissionsAttribute = Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => t.GetCustomAttributes<PermissionsAttribute>(inherit: false).Any());

            foreach (Type entity in entitiesWithPermissionsAttribute)
            {
                var requiredPermissions = entity.GetCustomAttribute<PermissionsAttribute>(false);

                foreach (Actions action in Enum.GetValues(typeof(Actions)))
                    if ((requiredPermissions.Actions & action) == action && action != Actions.CRUD)
                        allPermissions.Add(new ApplicationPermission
                        {
                            Value = $"{entity.Name}.{action}",
                            Name = $"{entity.Name} {action}",
                            GroupName = entity.Name
                        });
            }

            AllPermissions = allPermissions.AsReadOnly();
            AllPermissionsForTenants = allPermissions.Where(i => !i.Value.StartsWith("Tenant.")).ToList().AsReadOnly();
        }

        public ApplicationPermissions(TenantInfo tenantInfo)
        {
            IsMasterTenant = tenantInfo == null || tenantInfo.Id == Settings.DefaultTenantId;
        }

        private IEnumerable<ApplicationPermission> GetAllPermission()
        {
            return IsMasterTenant ? AllPermissions : AllPermissionsForTenants;
        }

        public ApplicationPermission GetPermissionByName(string permissionName)
        {
            return GetAllPermission().Where(p => p.Name == permissionName).FirstOrDefault();
        }

        public ApplicationPermission GetPermissionByValue(string permissionValue)
        {
            return GetAllPermission().Where(p => p.Value == permissionValue).FirstOrDefault();
        }

        public string[] GetAllPermissionValues()
        {
            return GetAllPermission().Select(p => p.Value).ToArray();
        }

        public string[] GetAllPermissionNames()
        {
            return GetAllPermission().Select(p => p.Name).ToArray();
        }
    }
}