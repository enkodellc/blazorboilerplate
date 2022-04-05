using Breeze.Sharp;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public partial class IdentityUserRole : BaseEntity
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

        public String Discriminator
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String TenantId
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

    }
}
