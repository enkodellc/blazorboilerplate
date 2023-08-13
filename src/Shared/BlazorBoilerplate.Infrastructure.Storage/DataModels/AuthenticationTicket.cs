using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [Permissions(Actions.CUD)]
    public class AuthenticationTicket
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        [JsonIgnore]
        public byte[] Value { get; set; }

        public DateTimeOffset? LastActivity { get; set; }

        public DateTimeOffset? Expires { get; set; }

        [MaxLength(46)]
        public string RemoteIpAddress { get; set; }

        [MaxLength(256)]
        public string OperatingSystem { get; set; }

        [MaxLength(256)] 
        public string UserAgentFamily { get; set; }

        [MaxLength(256)] 
        public string UserAgentVersion { get; set; }
    }
}
