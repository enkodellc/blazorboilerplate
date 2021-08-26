using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using BlazorBoilerplate.Infrastructure.Storage.DataInterfaces;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    
    public partial class Categories : IAuditable, ISoftDelete
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }

    }
}