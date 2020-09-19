using BlazorBoilerplate.Shared.Interfaces.Db;
using BlazorBoilerplate.Shared.SqlLocalizer;
using Breeze.Sharp;
using System;
using System.Collections.Generic;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public class LocalizationRecord : BaseEntity, ILocalizationRecord, IEquatable<LocalizationRecord>
    {
        public long Id
        {
            get { return GetValue<long>(); }
            set { SetValue(value); }
        }
        public string Key
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Text
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string LocalizationCulture
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string ResourceKey
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public LocalizationRecord()
        {
            LocalizationCulture = SqlLocalizer.Settings.NeutralCulture;
            ResourceKey = nameof(Global);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LocalizationRecord);
        }

        public bool Equals(LocalizationRecord other)
        {
            return other != null &&
                   Key == other.Key &&
                   LocalizationCulture == other.LocalizationCulture &&
                   ResourceKey == other.ResourceKey;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, LocalizationCulture, ResourceKey);
        }

        public static bool operator ==(LocalizationRecord left, LocalizationRecord right)
        {
            return EqualityComparer<LocalizationRecord>.Default.Equals(left, right);
        }

        public static bool operator !=(LocalizationRecord left, LocalizationRecord right)
        {
            return !(left == right);
        }
    }
}
