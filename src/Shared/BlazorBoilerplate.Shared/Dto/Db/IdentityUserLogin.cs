using Breeze.Sharp;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public partial class IdentityUserLogin : BaseEntity
    {

        public String Id
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String LoginProvider
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String ProviderDisplayName
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String ProviderKey
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String TenantId
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public Guid UserId
        {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }

    }
}
