using System.ComponentModel;

namespace BlazorBoilerplate.Shared.AuthorizationDefinitions
{
    public static class Actions
    {
        public const string Create = nameof(Create);
        public const string Read = nameof(Read);
        public const string Update = nameof(Update);
        public const string Delete = nameof(Delete);
    }

    public static class Permissions
    {
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

        public static class Role
        {
            [Description("Create a new Role")]
            public const string Create = nameof(Role) + "." + nameof(Actions.Create);

            [Description("Read roles data (permissions, etc.")]
            public const string Read = nameof(Role) + "." + nameof(Actions.Read);

            [Description("Edit existing Roles")]
            public const string Update = nameof(Role) + "." + nameof(Actions.Update);

            [Description("Delete any Role")]
            public const string Delete = nameof(Role) + "." + nameof(Actions.Delete);
        }

        public static class User
        {
            [Description("Create a new User")]
            public const string Create = nameof(User) + "." + nameof(Actions.Create);

            [Description("Read Users data (Names, Emails, Phone Numbers, etc.)")]
            public const string Read = nameof(User) + "." + nameof(Actions.Read);

            [Description("Edit existing users")]
            public const string Update = nameof(User) + "." + nameof(Actions.Update);

            [Description("Delete any user")]
            public const string Delete = nameof(User) + "." + nameof(Actions.Delete);
        }

        public static class WeatherForecasts
        {
            [Description("Read Weather Forecasts")]
            public const string Read = nameof(WeatherForecasts) + "." + nameof(Actions.Read);
        }

        public static class Tenant
        {
            [Description("Get a list of tenants and their details (users, Owner, etc.)")]
            public const string Read = nameof(Tenant) + "." + nameof(Actions.Read);

            [Description("Edit existing tenants")]
            public const string Update = nameof(Tenant) + "." + nameof(Actions.Update);

            [Description("Delete any tenant")]
            public const string Delete = nameof(Tenant) + "." + nameof(Actions.Delete);

            [Description("Holder considered as a tenant owner.")]
            public const string Manager = nameof(Tenant) + "." + nameof(Manager);
        }

        public static class Book
        {
            [Description("Create a new book")]
            public const string Create = nameof(Book) + "." + nameof(Actions.Create);

            [Description("Edit existing books")]
            public const string Update = nameof(Book) + "." + nameof(Actions.Update);

            [Description("Delete any book")]
            public const string Delete = nameof(Book) + "." + nameof(Actions.Delete);
        }
    }
}