using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto.Db;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [Permissions(Actions.CRUD)]
    public class TenantSetting
    {
        [Key]
        [Column(TypeName = "nvarchar(128)")]
        public SettingKey Key { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public SettingValue Value { get; set; }

        [Required]
        public SettingType Type { get; set; }
    }
}
