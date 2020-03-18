using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BlazorBoilerplate.Server.Data.Core
{
    public static class ApplicationPermissions
    {
        public static ReadOnlyCollection<ApplicationPermission> AllPermissions;
        /// <summary>
        /// Generates ApplicationPermissions based on Permissions Type by iterating over its nested classes and getting constant strings in each class as Value and Name, DescriptionAttribute of the constant string as Description, the nested class name as GroupName.
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
                    DescriptionAttribute[] attributes =
        (DescriptionAttribute[])permission.GetCustomAttributes(
        typeof(DescriptionAttribute),
        false);

                    if (attributes != null &&
                        attributes.Length > 0)
                    {
                        applicationPermission.Description = attributes[0].Description;
                    }
                    else
                    {
                        applicationPermission.Description = applicationPermission.Name;
                    }

                    allPermissions.Add(applicationPermission);
                }
            }
            AllPermissions = allPermissions.AsReadOnly();
        }

        public static ApplicationPermission GetPermissionByName(string permissionName)
        {
            return AllPermissions.Where(p => p.Name == permissionName).FirstOrDefault();
        }

        public static ApplicationPermission GetPermissionByValue(string permissionValue)
        {
            return AllPermissions.Where(p => p.Value == permissionValue).FirstOrDefault();
        }

        public static string[] GetAllPermissionValues()
        {
            return AllPermissions.Select(p => p.Value).ToArray();
        }

        public static string[] GetAllPermissionNames()
        {
            return AllPermissions.Select(p => p.Name).ToArray();
        }

        public static string[] GetAdministrativePermissionValues()
        {
            return GetAllPermissionNames();
        }
    }

    public class ApplicationPermission
    {
        public ApplicationPermission()
        { }

        public ApplicationPermission(string name, string value, string groupName, string description = null)
        {
            Name = name;
            Value = value;
            GroupName = groupName;
            Description = description;
        }

        public string Name { get; set; }
        public string Value { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator string(ApplicationPermission permission)
        {
            return permission.Value;
        }
    }
}