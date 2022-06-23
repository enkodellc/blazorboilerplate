Security
========
Administrator
-------------

In BlazorBoilerplate.Constants project change Administrator name to something less obvious than "admin".

::

    namespace BlazorBoilerplate.Constants
    {
        public static class DefaultUserNames
        {
            public const string Administrator = "iamtheboss";
            public const string User = "user";
        }
    }

In the same project reinforce password policy.

::

    namespace BlazorBoilerplate.Constants
    {
        public static class PasswordPolicy
            {
                public const bool RequireDigit = true;
                public const int RequiredLength = 8;
                public const bool RequireNonAlphanumeric = true;
                public const bool RequireUppercase = true;
                public const bool RequireLowercase = true;
            }
    }

In DatabaseInitializer in BlazorBoilerplate.Storage project change Administrator password "admin123" to satisfy new policy. 

::

     public async Task EnsureAdminIdentitiesAsync()
     {
        await EnsureRoleAsync(DefaultRoleNames.Administrator, _entityPermissions.GetAllPermissionValues());
        await CreateUserAsync(DefaultUserNames.Administrator, "X!PvG5+@", "Admin", "Blazor", "admin@blazorboilerplate.com", "+1 (123) 456-7890", new string[] { DefaultRoleNames.Administrator });


API endpoints
-------------
Imagine a malicious user writing or using a tool to call directly your API endpoints bypassing your UI, are your API endpoints protected by the right policy?

::

    [HttpGet]
    [Authorize(Policies.IsAdmin)]
    public IQueryable<ApplicationUser> Users()
    {
        return persistenceManager.GetEntities<ApplicationUser>().AsNoTracking().Include(i => i.UserRoles).ThenInclude(i => i.Role).OrderBy(i => i.UserName);
    }

What about the Breeze SaveChanges endpoint?

::

    [AllowAnonymous]
    [HttpPost]
    public SaveResult SaveChanges([FromBody] JObject saveBundle)
    ...

First of all remove [AllowAnonymous] attribute. An authenticated malicious user could post a proper json to update some EF entities.
To avoid this, decorate your EF entities with proper Permission attribute. E.g.

::

    namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
    {
        [Permissions(Actions.Delete)]
        public partial class Todo : IAuditable, ISoftDelete
    ...

Add the entity permissions to a role and put a user in this role.