using Breeze.Sharp;
using System;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public partial class ApplicationRole : BaseEntity
    {

        public Guid Id
        {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }

        public String ConcurrencyStamp
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String Name
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String NormalizedName
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String TenantId
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public NavigationSet<ApplicationUserRole> UserRoles
        {
            get { return GetValue<NavigationSet<ApplicationUserRole>>(); }
            set { SetValue(value); }
        }

   }
}
