using Breeze.Sharp;
using System;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public partial class UserProfile : BaseEntity
    {
        public UserProfile()
        {
            LastPageVisited = "/";
            IsNavOpen = true;
            IsNavMinified = false;
        }

        public String TenantId
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }
        public Int64 Id
        {
            get { return GetValue<Int64>(); }
            set { SetValue(value); }
        }

        public Int32 Count
        {
            get { return GetValue<Int32>(); }
            set { SetValue(value); }
        }

        public Boolean IsNavMinified
        {
            get { return GetValue<Boolean>(); }
            set { SetValue(value); }
        }

        public Boolean IsNavOpen
        {
            get { return GetValue<Boolean>(); }
            set { SetValue(value); }
        }

        public String LastPageVisited
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public DateTime LastUpdatedDate
        {
            get { return GetValue<DateTime>(); }
            set { SetValue(value); }
        }

        public Guid UserId
        {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }

        public ApplicationUser ApplicationUser
        {
            get { return GetValue<ApplicationUser>(); }
            set { SetValue(value); }
        }

   }
}
