using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [MultiTenant]
    [Permissions(Actions.CRUD)]
    public partial class ApplicationUser : IdentityUser<Guid>
    {
        public override Guid Id { get => base.Id; set => base.Id = value; }
        public override string UserName { get => base.UserName; set => base.UserName = value; }
        public override string NormalizedUserName { get => base.NormalizedUserName; set => base.NormalizedUserName = value; }
        public override string Email { get => base.Email; set => base.Email = value; }
        public override string NormalizedEmail { get => base.NormalizedEmail; set => base.NormalizedEmail = value; }
        public override bool EmailConfirmed { get => base.EmailConfirmed; set => base.EmailConfirmed = value; }
        public override string PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }
        public override bool PhoneNumberConfirmed { get => base.PhoneNumberConfirmed; set => base.PhoneNumberConfirmed = value; }
        public override bool TwoFactorEnabled { get => base.TwoFactorEnabled; set => base.TwoFactorEnabled = value; }
        public override DateTimeOffset? LockoutEnd { get => base.LockoutEnd; set => base.LockoutEnd = value; }
        public override bool LockoutEnabled { get => base.LockoutEnabled; set => base.LockoutEnabled = value; }
        public override int AccessFailedCount { get => base.AccessFailedCount; set => base.AccessFailedCount = value; }
        public override string ConcurrencyStamp { get => base.ConcurrencyStamp; set => base.ConcurrencyStamp = value; }

        [MaxLength(64)]
        public string FirstName { get; set; }

        [MaxLength(64)]
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        public ICollection<ApiLogItem> ApiLogItems { get; set; }

        public UserProfile Profile { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }
}