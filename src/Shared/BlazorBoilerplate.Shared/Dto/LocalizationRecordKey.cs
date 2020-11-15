using System;
using System.Collections.Generic;

namespace BlazorBoilerplate.Shared.Dto
{
    public class LocalizationRecordKey : IEquatable<LocalizationRecordKey>
    {
        public string ContextId { get; set; }
        public string MsgId { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as LocalizationRecordKey);
        }

        public bool Equals(LocalizationRecordKey other)
        {
            return other != null &&
                   ContextId == other.ContextId &&
                   MsgId == other.MsgId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ContextId, MsgId);
        }

        public override string ToString()
        {
            return $"{ContextId} - {MsgId}";
        }

        public static bool operator ==(LocalizationRecordKey left, LocalizationRecordKey right)
        {
            return EqualityComparer<LocalizationRecordKey>.Default.Equals(left, right);
        }

        public static bool operator !=(LocalizationRecordKey left, LocalizationRecordKey right)
        {
            return !(left == right);
        }
    }
}
