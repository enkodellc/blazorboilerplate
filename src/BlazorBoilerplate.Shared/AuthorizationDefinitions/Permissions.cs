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
            public const string Create = nameof(Todo) + "." + nameof(Actions.Create);
            public const string Read = nameof(Todo) + "." + nameof(Actions.Read);
            public const string Update = nameof(Todo) + "." + nameof(Actions.Update);
            public const string Delete = nameof(Todo) + "." + nameof(Actions.Delete);
        }
        public static class Role
        {
            public const string Create = nameof(Role) + "." + nameof(Actions.Create);
            public const string Read = nameof(Role) + "." + nameof(Actions.Read);
            public const string Update = nameof(Role) + "." + nameof(Actions.Update);
            public const string Delete = nameof(Role) + "." + nameof(Actions.Delete);
        }
        public static class User
        {
            public const string Create = nameof(User) + "." + nameof(Actions.Create);
            public const string Read = nameof(User) + "." + nameof(Actions.Read);
            public const string Update = nameof(User) + "." + nameof(Actions.Update);
            public const string Delete = nameof(User) + "." + nameof(Actions.Delete);
        }
    }
}
