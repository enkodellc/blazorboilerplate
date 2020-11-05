using BlazorBoilerplate.Shared.Localizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Server.Extensions
{
    public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
    {
        private readonly IStringLocalizer<Global> L;
        public LocalizedIdentityErrorDescriber(IStringLocalizer<Global> l)
        {
            L = l;
        }
        public override IdentityError DefaultError() => new IdentityError { Code = nameof(DefaultError), Description = L[nameof(DefaultError)] };
        public override IdentityError ConcurrencyFailure() => new IdentityError { Code = nameof(ConcurrencyFailure), Description = L[nameof(ConcurrencyFailure)] };
        public override IdentityError PasswordMismatch() => new IdentityError { Code = nameof(PasswordMismatch), Description = L[nameof(PasswordMismatch)] };
        public override IdentityError InvalidToken() => new IdentityError { Code = nameof(InvalidToken), Description = L[nameof(InvalidToken)] };
        public override IdentityError LoginAlreadyAssociated() => new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = L[nameof(LoginAlreadyAssociated)] };
        public override IdentityError InvalidUserName(string userName) => new IdentityError { Code = nameof(InvalidUserName), Description = L[nameof(InvalidUserName), userName] };
        public override IdentityError InvalidEmail(string email) => new IdentityError { Code = nameof(InvalidEmail), Description = L[nameof(InvalidEmail), email] };
        public override IdentityError DuplicateUserName(string userName) => new IdentityError { Code = nameof(DuplicateUserName), Description = L[nameof(DuplicateUserName), userName] };
        public override IdentityError DuplicateEmail(string email) => new IdentityError { Code = nameof(DuplicateEmail), Description = L[nameof(DuplicateEmail), email] };
        public override IdentityError InvalidRoleName(string role) => new IdentityError { Code = nameof(InvalidRoleName), Description = L[nameof(InvalidRoleName), role] };
        public override IdentityError DuplicateRoleName(string role) => new IdentityError { Code = nameof(DuplicateRoleName), Description = L[nameof(DuplicateRoleName), role] };
        public override IdentityError UserAlreadyHasPassword() => new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = L[nameof(UserAlreadyHasPassword)] };
        public override IdentityError UserLockoutNotEnabled() => new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = L[nameof(UserLockoutNotEnabled)] };
        public override IdentityError UserAlreadyInRole(string role) => new IdentityError { Code = nameof(UserAlreadyInRole), Description = L[nameof(UserAlreadyInRole), role] };
        public override IdentityError UserNotInRole(string role) => new IdentityError { Code = nameof(UserNotInRole), Description = L[nameof(UserNotInRole), role] };
        public override IdentityError PasswordTooShort(int length) => new IdentityError { Code = nameof(PasswordTooShort), Description = L[nameof(PasswordTooShort), length] };
        public override IdentityError PasswordRequiresNonAlphanumeric() => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = L[nameof(PasswordRequiresNonAlphanumeric)] };
        public override IdentityError PasswordRequiresDigit() => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = L[nameof(PasswordRequiresDigit)] };
        public override IdentityError PasswordRequiresLower() => new IdentityError { Code = nameof(PasswordRequiresLower), Description = L[nameof(PasswordRequiresLower)] };
        public override IdentityError PasswordRequiresUpper() => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = L[nameof(PasswordRequiresUpper)] };
    }
}
