namespace BlazorBoilerplate.Shared.Dto.ExternalAuth
{
    public enum ErrorEnum
    {
        Unknown = -99,
        UserCreationFailed = -1,
        UserIsNotAllowed = 0,
        UserLockedOut = 1,
        CannotAddExternalLogin = 2,
        ExternalAuthError = 3,
        ExternalUnknownUserId = 4,
        ProviderNotFound = 5,
        Domain = 6,
    }
}
