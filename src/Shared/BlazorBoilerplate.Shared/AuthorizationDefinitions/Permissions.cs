
using System.ComponentModel;
using BlazorBoilerplate.Localization;

namespace BlazorBoilerplate.Shared.AuthorizationDefinitions
{
    public static class Actions
    {
        public const string Create = nameof(Create);
        public const string Update = nameof(Update);
        public const string Read = nameof(Read);
        public const string Delete = nameof(Delete);
    }

    public static class Permissions
    {
        #region Admin
        public static class User
        {
            [LocalizedDescription("CreateUserPermission", typeof(Strings))]
            public const string Create = nameof(User) + "." + nameof(Actions.Create);
            [LocalizedDescription("UpdateUserPermission", typeof(Strings))]
            public const string Update = nameof(User) + "." + nameof(Actions.Update);
            [LocalizedDescription("ReadUserPermission", typeof(Strings))]
            public const string Read = nameof(User) + "." + nameof(Actions.Read);
            [LocalizedDescription("DeleteUserPermission", typeof(Strings))]
            public const string Delete = nameof(User) + "." + nameof(Actions.Delete);
        }
        public static class Role
        {
            [LocalizedDescription("CreateRolePermission", typeof(Strings))]
            public const string Create = nameof(Role) + "." + nameof(Actions.Create);
            [LocalizedDescription("UpdateRolePermission", typeof(Strings))]
            public const string Update = nameof(Role) + "." + nameof(Actions.Update);
            [LocalizedDescription("ReadRolePermission", typeof(Strings))]
            public const string Read = nameof(Role) + "." + nameof(Actions.Read);
            [LocalizedDescription("DeleteRolePermission", typeof(Strings))]
            public const string Delete = nameof(Role) + "." + nameof(Actions.Delete);
        }
        public static class Client
        {
            [LocalizedDescription("CreateClientPermission", typeof(Strings))]
            public const string Create = nameof(Client) + "." + nameof(Actions.Create);
            [LocalizedDescription("UpdateClientPermission", typeof(Strings))]
            public const string Update = nameof(Client) + "." + nameof(Actions.Update);
            [LocalizedDescription("ReadClientPermission", typeof(Strings))]
            public const string Read = nameof(Client) + "." + nameof(Actions.Read);
            [LocalizedDescription("DeleteClientPermission", typeof(Strings))]
            public const string Delete = nameof(Client) + "." + nameof(Actions.Delete);
        }
        public static class ApiResource
        {
            [LocalizedDescription("CreateApiResourcePermission", typeof(Strings))]
            public const string Create = nameof(ApiResource) + "." + nameof(Actions.Create);
            [LocalizedDescription("UpdateApiResourcePermission", typeof(Strings))]
            public const string Update = nameof(ApiResource) + "." + nameof(Actions.Update);
            [LocalizedDescription("ReadApiResourcePermission", typeof(Strings))]
            public const string Read = nameof(ApiResource) + "." + nameof(Actions.Read);
            [LocalizedDescription("DeleteApiResourcePermission", typeof(Strings))]
            public const string Delete = nameof(ApiResource) + "." + nameof(Actions.Delete);
        }
        public static class IdentityResource
        {
            [LocalizedDescription("CreateIdentityResourcePermission", typeof(Strings))]
            public const string Create = nameof(IdentityResource) + "." + nameof(Actions.Create);
            [LocalizedDescription("UpdateIdentityResourcePermission", typeof(Strings))]
            public const string Update = nameof(IdentityResource) + "." + nameof(Actions.Update);
            [LocalizedDescription("ReadIdentityResourcePermission", typeof(Strings))]
            public const string Read = nameof(IdentityResource) + "." + nameof(Actions.Read);
            [LocalizedDescription("DeleteIdentityResourcePermission", typeof(Strings))]
            public const string Delete = nameof(IdentityResource) + "." + nameof(Actions.Delete);
        }
        #endregion
        
        public static class Todo
        {
            [Description("Create a new ToDo")]
            public const string Create = nameof(Todo) + "." + nameof(Actions.Create);
            [Description("Read ToDos")]
            public const string Read = nameof(Todo) + "." + nameof(Actions.Read);
            [Description("Edit existing ToDos")]
            public const string Update = nameof(Todo) + "." + nameof(Actions.Update);
            [Description("Delete any ToDo")]
            public const string Delete = nameof(Todo) + "." + nameof(Actions.Delete);
        }
    }
}