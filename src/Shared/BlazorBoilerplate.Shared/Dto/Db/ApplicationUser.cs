using Breeze.Sharp;
using System;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public partial class ApplicationUser : BaseEntity
    {

        public Guid Id
        {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }

        public Int32 AccessFailedCount
        {
            get { return GetValue<Int32>(); }
            set { SetValue(value); }
        }

        public String ConcurrencyStamp
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String Email
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public Boolean EmailConfirmed
        {
            get { return GetValue<Boolean>(); }
            set { SetValue(value); }
        }

        public String FirstName
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String FullName
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String LastName
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public Boolean LockoutEnabled
        {
            get { return GetValue<Boolean>(); }
            set { SetValue(value); }
        }

        public DateTimeOffset? LockoutEnd
        {
            get { return GetValue<DateTimeOffset?>(); }
            set { SetValue(value); }
        }

        public String NormalizedEmail
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String NormalizedUserName
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String PasswordHash
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String PhoneNumber
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public Boolean PhoneNumberConfirmed
        {
            get { return GetValue<Boolean>(); }
            set { SetValue(value); }
        }

        public String SecurityStamp
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String TenantId
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public Boolean TwoFactorEnabled
        {
            get { return GetValue<Boolean>(); }
            set { SetValue(value); }
        }

        public String UserName
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public NavigationSet<ApiLogItem> ApiLogItems
        {
            get { return GetValue<NavigationSet<ApiLogItem>>(); }
            set { SetValue(value); }
        }

        public NavigationSet<Message> Messages
        {
            get { return GetValue<NavigationSet<Message>>(); }
            set { SetValue(value); }
        }

        public UserProfile Profile
        {
            get { return GetValue<UserProfile>(); }
            set { SetValue(value); }
        }

        public NavigationSet<ApplicationUserRole> UserRoles
        {
            get { return GetValue<NavigationSet<ApplicationUserRole>>(); }
            set { SetValue(value); }
        }

   }
}
