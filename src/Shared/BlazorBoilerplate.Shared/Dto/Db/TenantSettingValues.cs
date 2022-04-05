using BlazorBoilerplate.Constants;
using System.Security.Authentication;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public enum BlazorRuntime
    {
        Server,
        WebAssembly
    }
    public static class TenantSettingValues
    {
        public static readonly Dictionary<SettingKey, Tuple<string, SettingType>> Default =
                new()
                {
                    { SettingKey.MainConfiguration_Runtime, new Tuple<string, SettingType>(BlazorRuntime.Server.ToString(), SettingType.String) },

                    { SettingKey.EmailConfiguration_SmtpServer, new Tuple<string, SettingType>("smtp.gmail.com", SettingType.String) },
                    { SettingKey.EmailConfiguration_SmtpPort, new Tuple<string, SettingType>("465", SettingType.Int) },
                    { SettingKey.EmailConfiguration_SmtpUsername, new Tuple<string, SettingType>("email@domain.com", SettingType.String) },
                    { SettingKey.EmailConfiguration_SmtpPassword, new Tuple<string, SettingType>("PASSWORD", SettingType.String) },
                    { SettingKey.EmailConfiguration_SmtpUseSSL, new Tuple<string, SettingType>("true", SettingType.Boolean) },
                    { SettingKey.EmailConfiguration_SmtpSslProtocol, new Tuple<string, SettingType>(SslProtocols.Tls13.ToString(), SettingType.String) },

                    { SettingKey.EmailConfiguration_FromName, new Tuple<string, SettingType>("BlazorBoilerplate Team", SettingType.String) },
                    { SettingKey.EmailConfiguration_FromAddress, new Tuple<string, SettingType>("email@domain.com", SettingType.String) },
                    { SettingKey.EmailConfiguration_ReplyToAddress, new Tuple<string, SettingType>("email@domain.com", SettingType.String) },
                    { SettingKey.EmailConfiguration_PopServer, new Tuple<string, SettingType>("pop.gmail.com", SettingType.String) },
                    { SettingKey.EmailConfiguration_PopPort, new Tuple<string, SettingType>("995", SettingType.Int) },
                    { SettingKey.EmailConfiguration_PopUsername, new Tuple<string, SettingType>("email@domain.com", SettingType.String) },
                    { SettingKey.EmailConfiguration_PopPassword, new Tuple<string, SettingType>("PASSWORD", SettingType.String) },
                    { SettingKey.EmailConfiguration_PopUseSSL, new Tuple<string, SettingType>("true", SettingType.Boolean) },

                    { SettingKey.EmailConfiguration_ImapServer, new Tuple<string, SettingType>("imap.gmail.com", SettingType.String) },
                    { SettingKey.EmailConfiguration_ImapPort, new Tuple<string, SettingType>("993", SettingType.Int) },
                    { SettingKey.EmailConfiguration_ImapUsername, new Tuple<string, SettingType>("email@domain.com", SettingType.String) },
                    { SettingKey.EmailConfiguration_ImapPassword, new Tuple<string, SettingType>("PASSWORD", SettingType.String) },
                    { SettingKey.EmailConfiguration_ImapUseSSL, new Tuple<string, SettingType>("true", SettingType.Boolean) },
                };
    }
}
