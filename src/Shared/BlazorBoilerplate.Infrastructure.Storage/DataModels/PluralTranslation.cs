using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using BlazorBoilerplate.Shared.DataInterfaces;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [Permissions(Actions.Create | Actions.Update | Actions.Delete)]
    public class PluralTranslation : IPluralTranslation
    {
        [Key]
        public long Id { get; set; }

        public int Index { get; set; }

        [Required]
        public string Translation { get; set; }

        //Entity Framework generates the following key as shadow property https://docs.microsoft.com/en-us/ef/core/modeling/shadow-properties
        //Breeze Sharp DTO Entity needs this key. I explicitly add the key, so running BlazorBoilerplate.EntityGenerator I have the right entity
        public long LocalizationRecordId { get; set; }

        [Required]
        public LocalizationRecord LocalizationRecord { get; set; }
        ILocalizationRecord IPluralTranslation.LocalizationRecord { get => LocalizationRecord; set => LocalizationRecord = (LocalizationRecord)value; }
    }
}
