using Breeze.Sharp;
using System;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public partial class Message : BaseEntity
    {

        public Int32 Id
        {
            get { return GetValue<Int32>(); }
            set { SetValue(value); }
        }

        public String Text
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public Guid UserID
        {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }

        public String UserName
        {
            get { return GetValue<String>(); }
            set { SetValue(value); }
        }

        public DateTime When
        {
            get { return GetValue<DateTime>(); }
            set { SetValue(value); }
        }

        public ApplicationUser Sender
        {
            get { return GetValue<ApplicationUser>(); }
            set { SetValue(value); }
        }

   }
}
