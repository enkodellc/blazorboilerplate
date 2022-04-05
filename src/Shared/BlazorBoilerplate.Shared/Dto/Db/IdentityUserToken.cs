using Breeze.Sharp;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public partial class IdentityUserToken : BaseEntity
    {

        public Guid UserId
        {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }

        public String LoginProvider
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String Name
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String TenantId
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public String Value
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

    }
}
