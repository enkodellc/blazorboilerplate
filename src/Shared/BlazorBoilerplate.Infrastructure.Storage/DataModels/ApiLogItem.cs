using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [Permissions(Actions.CRUD)]
    public partial class ApiLogItem
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "FieldRequired")]
        public DateTime RequestTime { get; set; }

        [Required(ErrorMessage = "FieldRequired")]
        public long ResponseMillis { get; set; }

        [Required(ErrorMessage = "FieldRequired")]
        public int StatusCode { get; set; }

        [Required(ErrorMessage = "FieldRequired")]
        public string Method { get; set; }

        [Required(ErrorMessage = "FieldRequired")]
        [MaxLength(2048)]
        public string Path { get; set; }

        [MaxLength(2048)]
        public string QueryString { get; set; }

        [MaxLength(256)]
        public string RequestBody { get; set; }

        [MaxLength(256)]
        public string ResponseBody { get; set; }

        [MaxLength(45)]
        public string IPAddress { get; set; }

        public Guid? ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}
