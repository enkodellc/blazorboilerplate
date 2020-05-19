using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Infrastructure.Storage.DataInterfaces;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [Permissions(Actions.Delete)]
    public class Todo : IAuditable, ISoftDelete
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Title { get; set; }

        public bool IsCompleted { get; set; }
    }
}