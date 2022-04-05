using Finbuckle.MultiTenant;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [MultiTenant]
    public partial class UserProfile
    {
        [Key]
        public long Id { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public string LastPageVisited { get; set; } = "/";
        public bool IsNavOpen { get; set; } = true;
        public bool IsNavMinified { get; set; } = false;
        public int Count { get; set; } = 0;
        public DateTime LastUpdatedDate { get; set; } = DateTime.MinValue;

        [Column(TypeName = "nvarchar(64)")]
        public string TenantId { get; set; }
    }
}
