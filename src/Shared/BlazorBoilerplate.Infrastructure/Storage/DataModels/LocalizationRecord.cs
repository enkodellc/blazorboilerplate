using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Interfaces.Db;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [Permissions(Actions.Create | Actions.Update | Actions.Delete)]
    public partial class LocalizationRecord : ILocalizationRecord
    {
        [Key]
        public long Id { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public string LocalizationCulture { get; set; }
        public string ResourceKey { get; set; }
    }
}
