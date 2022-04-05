using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using BlazorBoilerplate.Shared.DataInterfaces;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [Permissions(Actions.Create | Actions.Update | Actions.Delete)]
    public partial class LocalizationRecord : ILocalizationRecord
    {
        [Key]
        public long Id { get; set; }
        public string MsgId { get; set; }
        public string MsgIdPlural { get; set; }
        public string Translation { get; set; }
        public string Culture { get; set; }
        public string ContextId { get; set; }
        public ICollection<PluralTranslation> PluralTranslations { get; set; }
        ICollection<IPluralTranslation> ILocalizationRecord.PluralTranslations { get => PluralTranslations?.Select(i => (IPluralTranslation)i).ToList(); }
    }
}
