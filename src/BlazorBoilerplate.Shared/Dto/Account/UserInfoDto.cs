using System.Collections.Generic;

namespace BlazorBoilerplate.Shared.Dto
{
    public class UserInfoDto
    {
        public bool IsAuthenticated { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string[] Roles { get; set; }
        public Dictionary<string, string> ExposedClaims { get; set; }
    }

    //For testing Admin UI
    public class DemoUserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FistName { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }

        public DemoUserDto()
        {
        }
    }
}
