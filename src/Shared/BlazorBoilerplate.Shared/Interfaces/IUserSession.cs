namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface IUserSession
    {
        Guid UserId { get; set; }
        string UserName { get; set; }
        List<string> Roles { get; set; }
        List<KeyValuePair<string, string>> ExposedClaims { get; set; }
    }
}