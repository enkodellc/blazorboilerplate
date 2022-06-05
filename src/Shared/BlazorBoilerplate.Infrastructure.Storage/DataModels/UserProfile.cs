using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    public partial class UserProfile
    {
        [Key]
        public long Id { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public string LastPageVisited { get; set; } = "/";
        public bool IsNavOpen { get; set; } = true;
        public bool IsNavMinified { get; set; } = false;
        public DateTime LastUpdatedDate { get; set; } = DateTime.MinValue;
        public string Culture { get; set; }
    }
}
