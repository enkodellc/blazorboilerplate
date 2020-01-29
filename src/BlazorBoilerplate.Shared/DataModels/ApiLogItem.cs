using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.DataModels
{
    public class ApiLogItem
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public DateTime RequestTime { get; set; }

        [Required]
        public long ResponseMillis { get; set; }

        [Required]
        public int StatusCode { get; set; }

        [Required]
        public string Method { get; set; }

        [Required]
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
    }
}
