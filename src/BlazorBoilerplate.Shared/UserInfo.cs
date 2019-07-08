using System.Collections.Generic;

namespace BlazorBoilerplate.Shared
{
    public class UserInfo
    {
        public bool IsAuthenticated { get; set; }
        public string Username { get; set; }
        public Dictionary<string, string> ExposedClaims { get; set; }
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
