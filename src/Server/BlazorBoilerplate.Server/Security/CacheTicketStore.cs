using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Server.Extensions;
using BlazorBoilerplate.Shared.Dto.Email;
using BlazorBoilerplate.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BlazorBoilerplate.Server.Security;

public class CacheTicketStore : ITicketStore
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private IDataProtector _dataProtector;
    private ILogger<CacheTicketStore> _logger;

    public CacheTicketStore(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IDataProtector DataProtector => _dataProtector ??= _httpContextAccessor.HttpContext.RequestServices.GetService<IDataProtectionProvider>()
        .CreateProtector($"{nameof(CacheTicketStore)}_{IdentityConstants.ApplicationScheme}");
    public ILogger<CacheTicketStore> Logger => _logger ??= _httpContextAccessor.HttpContext.RequestServices.GetService<ILogger<CacheTicketStore>>();

    public async Task RemoveAsync(string key)
    {
        var context = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();

        if (Guid.TryParse(key, out var id))
        {
            var ticket = await context.AuthenticationTickets.SingleOrDefaultAsync(x => x.Id == id);

            if (ticket != null)
            {
                context.AuthenticationTickets.Remove(ticket);

                await context.SaveChangesAsync();
            }
        }
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var context = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();

        try
        {
            if (Guid.TryParse(key, out var id))
            {
                var authenticationTicket = await context.AuthenticationTickets.FindAsync(id);

                if (authenticationTicket != null)
                {
                    authenticationTicket.Value = DataProtector.Protect(SerializeTicket(ticket));
                    authenticationTicket.LastActivity = DateTimeOffset.UtcNow;
                    authenticationTicket.Expires = ticket.Properties.ExpiresUtc;

                    await context.SaveChangesAsync();
                }
            }
        }
        catch (Exception e)
        {
            // Data Protection Error
            Logger.LogError(e, "{methodName} failed  for '{key}'.", nameof(RenewAsync), key);
        }
    }

    public async Task<AuthenticationTicket> RetrieveAsync(string key)
    {
        var context = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();

        try
        {
            if (Guid.TryParse(key, out var id))
            {
                var authenticationTicket = await context.AuthenticationTickets.FindAsync(id);

                if (authenticationTicket != null)
                {
                    authenticationTicket.LastActivity = DateTimeOffset.UtcNow;

                    context.AuthenticationTickets.RemoveRange(context.AuthenticationTickets.Where(i => i.Expires < DateTimeOffset.UtcNow));

                    await context.SaveChangesAsync();

                    return DeserializeTicket(DataProtector.Unprotect(authenticationTicket.Value));
                }
            }
        }
        catch (Exception e)
        {
            // Data Protection Error
            Logger.LogError(e, "{methodName} failed  for '{key}'.", nameof(RetrieveAsync), key);
        }


        return null;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        Guid userId = ticket.Principal.GetUserId();
        var context = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();
        var emailFactory = _httpContextAccessor.HttpContext.RequestServices.GetService<IEmailFactory>();
        var emailManager = _httpContextAccessor.HttpContext.RequestServices.GetService<IEmailManager>();

        try
        {
            var authenticationTicket = new Infrastructure.Storage.DataModels.AuthenticationTicket()
            {
                UserId = userId,
                LastActivity = DateTimeOffset.UtcNow,
                Value = DataProtector.Protect(SerializeTicket(ticket))
            };

            var expiresUtc = ticket.Properties.ExpiresUtc;

            if (expiresUtc.HasValue)
                authenticationTicket.Expires = expiresUtc.Value;

            var httpContext = _httpContextAccessor?.HttpContext;

            if (httpContext != null)
            {
                var remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();

                if (remoteIpAddress != null)
                {
                    authenticationTicket.RemoteIpAddress = remoteIpAddress;
                }

                var userAgent = httpContext.Request.Headers["User-Agent"];

                if (!string.IsNullOrEmpty(userAgent))
                {
                    var uaParser = UAParser.Parser.GetDefault();
                    var clientInfo = uaParser.Parse(userAgent);
                    authenticationTicket.OperatingSystem = clientInfo.OS.ToString();
                    authenticationTicket.UserAgentFamily = clientInfo.UA.Family;
                    authenticationTicket.UserAgentVersion = $"{clientInfo.UA.Major}.{clientInfo.UA.Minor}.{clientInfo.UA.Patch}";

                    context.AuthenticationTickets.RemoveRange(context.AuthenticationTickets.Where(i => i.UserId == userId && i.OperatingSystem == null));

                    await context.SaveChangesAsync();

                    if (remoteIpAddress != null)
                    {
                        if (!await context.AuthenticationTickets.Where(i => i.UserId == userId && i.RemoteIpAddress == remoteIpAddress).AnyAsync())
                        {
                            var user = await context.Users.Include(i => i.Person).Include(i => i.Profile).SingleAsync(i => i.Id == userId);

                            CultureInfo.CurrentCulture = new CultureInfo(user.Profile?.Culture ?? "en-US");
                            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;

                            var email = emailFactory.BuildNewAccessEmail(user.Name, user.UserName, remoteIpAddress, authenticationTicket.LastActivity.Value);

                            email.ToAddresses.Add(new EmailAddressDto(user.Name, user.Email));

                            await emailManager.QueueEmail(email, EmailType.NewAccess);
                        }
                    }
                }
            }

            context.AuthenticationTickets.Add(authenticationTicket);

            await context.SaveChangesAsync();

            return authenticationTicket.Id.ToString();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "{methodName} failed  for '{key}'.", nameof(StoreAsync), userId);
            return null;
        }
    }

    private byte[] SerializeTicket(AuthenticationTicket source)
        => TicketSerializer.Default.Serialize(source);

    private AuthenticationTicket DeserializeTicket(byte[] source)
        => source == null ? null : TicketSerializer.Default.Deserialize(source);
}
