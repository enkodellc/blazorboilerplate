﻿using Microsoft.AspNetCore.Authorization;

//This is ASP.Net Core Identity
//see https://chrissainty.com/securing-your-blazor-apps-configuring-policy-based-authorization-with-blazor/

namespace BlazorBoilerplate.Shared.AuthorizationDefinitions
{
    public static class Policies
    {
        public const string IsAdmin = "IsAdmin";
        public const string IsUser = "IsUser";
        public const string IsReadOnly = "IsReadOnly";
        public const string IsMyDomain = "IsMyDomain";
        public const string TwoFactorEnabled = "TwoFactorEnabled";

        public static AuthorizationPolicy IsAdminPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("IsAdministrator")
                .Build();
        }

        public static AuthorizationPolicy IsUserPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("IsUser")
                .Build();
        }

        public static AuthorizationPolicy IsReadOnlyPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("ReadOnly", "true")
                .Build();
        }

        //https://docs.microsoft.com/it-it/aspnet/core/security/authentication/mfa
        public static AuthorizationPolicy IsTwoFactorEnabledPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("amr", "mfa")
                .Build();
        }

        public static AuthorizationPolicy IsMyDomainPolicy()
        {
            return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new DomainRequirement("blazorboilerplate.com"))
            .Build();                
        }
    }
}
