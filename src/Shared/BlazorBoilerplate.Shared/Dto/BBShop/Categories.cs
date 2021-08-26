using BlazorBoilerplate.Shared.Helpers;
using Breeze.Sharp;
using System.Collections.Generic;

namespace BlazorBoilerplate.Shared.Dto.BBShop
{
    public partial class Categories : BaseEntity
    {
        public int Id
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }

        public string Name
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public string Url
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public string Icon
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
     
    }
}
