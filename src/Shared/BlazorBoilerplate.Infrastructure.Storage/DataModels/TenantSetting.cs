using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using Finbuckle.MultiTenant;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [Permissions(Actions.CRUD)]
    [MultiTenant]
    public partial class TenantSetting
    {
        [MaxLength(64)]
        public string TenantId { get; set; }

        [MaxLength(128)]
        public SettingKey Key { get; set; }

        [Column(TypeName = "text")]
        public string Value { get; set; }

        [Required(ErrorMessage = "FieldRequired")]
        public SettingType Type { get; set; }
    }
}
