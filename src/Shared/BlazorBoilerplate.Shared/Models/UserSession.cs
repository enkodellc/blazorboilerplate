using BlazorBoilerplate.Shared.Interfaces;

namespace BlazorBoilerplate.Shared.Models
{
    public class UserSession : IUserSession
    {
        public bool IsAuthenticated { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; }
        public List<KeyValuePair<string, string>> ExposedClaims { get; set; }

        public UserSession()
        {
        }

        public UserSession(Guid userId, string userName)
        {
            UserId = userId;
            UserName = userName;
        }
    }
}
