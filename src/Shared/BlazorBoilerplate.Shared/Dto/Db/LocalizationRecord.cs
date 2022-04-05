using BlazorBoilerplate.Shared.Localizer;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public partial class LocalizationRecord : IEquatable<LocalizationRecord>
    {
        public LocalizationRecord()
        {
            Culture = Settings.NeutralCulture;
            ContextId = nameof(Global);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LocalizationRecord);
        }

        public bool Equals(LocalizationRecord other)
        {
            return other != null &&
                   MsgId == other.MsgId &&
                   Culture == other.Culture &&
                   ContextId == other.ContextId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MsgId, Culture, ContextId);
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
