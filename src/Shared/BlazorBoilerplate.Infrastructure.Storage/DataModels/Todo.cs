using BlazorBoilerplate.Infrastructure.Storage.DataInterfaces;
using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [Permissions(Actions.Delete)]
    public partial class Todo : IAuditable, ISoftDelete
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "FieldRequired")]
        [MaxLength(128)]
        public string Title { get; set; }

        public bool IsCompleted { get; set; }
    }
}