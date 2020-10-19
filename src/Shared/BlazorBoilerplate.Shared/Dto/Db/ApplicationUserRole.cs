using Breeze.Sharp;
using System;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public partial class ApplicationUserRole : BaseEntity
    {
        public Guid UserId
        {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }

        public Guid RoleId
        {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }

        public String TenantId
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public ApplicationRole Role
        {
            get { return GetValue<ApplicationRole>(); }
            set { SetValue(value); }
        }

        public ApplicationUser User
        {
            get { return GetValue<ApplicationUser>(); }
            set { SetValue(value); }
        }

   }
}
