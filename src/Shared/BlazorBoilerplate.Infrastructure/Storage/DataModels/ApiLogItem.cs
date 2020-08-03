using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Localization;
using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [Permissions(Actions.CRUD)]
    public class ApiLogItem
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        public DateTime RequestTime { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        public long ResponseMillis { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        public int StatusCode { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        public string Method { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
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
