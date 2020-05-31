using System;
using System.Collections.Generic;
using System.Globalization;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public class SettingValue
    {
        private string value;

        public SettingValue() { }

        public SettingValue(string value) => this.value = value;

        public static implicit operator string(SettingValue i) { return i.value; }

        public static implicit operator int(SettingValue i) { return int.Parse(i.value); }

        public static implicit operator byte(SettingValue i) { return byte.Parse(i.value); }

        public static implicit operator decimal(SettingValue i) { return decimal.Parse(i.value.Replace(" ", string.Empty).Replace(",", "."), CultureInfo.InvariantCulture); }

        public static implicit operator bool(SettingValue i) { return bool.Parse(i.value); }

        public static implicit operator SettingValue(string s) { return new SettingValue(s); }

        public override string ToString()
        {
            return value;
        }

        public static readonly Dictionary<SettingKey, Tuple<string, SettingType>> DefaultValues =
                new Dictionary<SettingKey, Tuple<string, SettingType>>()
                {
                { SettingKey.EmailConfiguration_SmtpServer,     new Tuple<string, SettingType>("smtp.gmail.com", SettingType.String) },
                { SettingKey.EmailConfiguration_SmtpPort,       new Tuple<string, SettingType>("465", SettingType.Int) },
                { SettingKey.EmailConfiguration_SmtpUsername,   new Tuple<string, SettingType>("email@domain.com", SettingType.String) },
                { SettingKey.EmailConfiguration_SmtpPassword,   new Tuple<string, SettingType>("PASSWORD", SettingType.String) },
                { SettingKey.EmailConfiguration_SmtpUseSSL,     new Tuple<string, SettingType>("true", SettingType.Bool) },
                { SettingKey.EmailConfiguration_FromName,       new Tuple<string, SettingType>("BlazorBoilerplate Team", SettingType.String) },
                { SettingKey.EmailConfiguration_FromAddress,    new Tuple<string, SettingType>("email@domain.com", SettingType.String) },
                { SettingKey.EmailConfiguration_ReplyToAddress, new Tuple<string, SettingType>("email@domain.com", SettingType.String) },
                { SettingKey.EmailConfiguration_PopServer,      new Tuple<string, SettingType>("pop.gmail.com", SettingType.String) },
                { SettingKey.EmailConfiguration_PopPort,        new Tuple<string, SettingType>("995", SettingType.Int) },
                { SettingKey.EmailConfiguration_PopUsername,    new Tuple<string, SettingType>("email@domain.com", SettingType.String) },
                { SettingKey.EmailConfiguration_PopPassword,    new Tuple<string, SettingType>("PASSWORD", SettingType.String) },
                { SettingKey.EmailConfiguration_PopUseSSL,      new Tuple<string, SettingType>("true", SettingType.Bool) },
                { SettingKey.EmailConfiguration_ImapServer,     new Tuple<string, SettingType>("imap.gmail.com", SettingType.String) },
                { SettingKey.EmailConfiguration_ImapPort,       new Tuple<string, SettingType>("993", SettingType.Int) },
                { SettingKey.EmailConfiguration_ImapUsername,   new Tuple<string, SettingType>("email@domain.com", SettingType.String) },
                { SettingKey.EmailConfiguration_ImapPassword,   new Tuple<string, SettingType>("PASSWORD", SettingType.String) },
                { SettingKey.EmailConfiguration_ImapUseSSL,     new Tuple<string, SettingType>("true", SettingType.Bool) },
                };
    }
}
