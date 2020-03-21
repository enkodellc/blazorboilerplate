namespace BlazorBoilerplate.Storage.Core
{
    public static class ClaimConstants
    {
        ///<summary>A claim that specifies the subject of an entity</summary>
        public const string Subject = "sub";

        ///<summary>A claim that specifies the permission of an entity</summary>
        public const string Permission = "permission";

        /// <summary>A claim that specifies the Id of the tenant which the holder belongs to. </summary>
        public const string TenantId = nameof(TenantId);
    }

    public static class ScopeConstants
    {
        ///<summary>A scope that specifies the roles of an entity</summary>
        public const string Roles = "roles";
    }

    public static class TenantConstants
    {
        /// <summary>Title of the root tenant.</summary>
        public const string RootTenantTitle = "Root";
    }

    public static class RoleConstants
    {
        public const string AdminRoleName = "Administrator";
        public const string UserRoleName = "User";
        public const string TenantManagerRoleName = "TenantManager";
    }
}