using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBoilerplate.Shared
{
    public class UserInfo
    {
        public bool   IsAuthenticated { get; set; }
        public string Username        { get; set; }
    }

    //For testing Admin UI
    public class DemoUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FistName { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }

        public DemoUser()
        {
        }
    }
}
