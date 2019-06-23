using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace BlazorBoilerplate.Server.Helpers
{
    public static class EmailTemplates
    {
        static IWebHostEnvironment _webHostEnvironment;
        static string testEmailTemplate;
        static string plainTextTestEmailTemplate;
        static string newUserEmailTemplate;
        static string newUserNotificationEmailTemplate;
        static string passwordResetTemplate;
        static string forgotPasswordTemplate;

        public static void Initialize(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public static string GetTestEmail(string recepientName, DateTime testDate)
        {
            if (testEmailTemplate == null)
                testEmailTemplate = ReadPhysicalFile("Helpers/Templates/TestEmail.template");

            string emailMessage = testEmailTemplate
                .Replace("{user}", recepientName)
                .Replace("{testDate}", testDate.ToString());

            return emailMessage;
        }

        public static string GetPlainTextTestEmail(DateTime date)
        {
            if (plainTextTestEmailTemplate == null)
                plainTextTestEmailTemplate = ReadPhysicalFile("Helpers/Templates/PlainTextTestEmail.template");

            string emailMessage = plainTextTestEmailTemplate
                .Replace("{date}", date.ToString());

            return emailMessage;
        }

        public static string GetNewUserEmail(string recepientName, string userName, string password)
        {
            if (newUserEmailTemplate == null)
                newUserEmailTemplate = ReadPhysicalFile("Helpers/Templates/NewUserEmail.template");

            string emailMessage = newUserEmailTemplate
                .Replace("{name}", recepientName)
                .Replace("{userName}", userName)
                .Replace("{password}", password);

            return emailMessage;
        }

        public static string GetNewUserNotificationEmail(string creator, string name, string userName, string company, string roles)
        {
            if (newUserNotificationEmailTemplate == null)
                newUserNotificationEmailTemplate = ReadPhysicalFile("Helpers/Templates/NewUserEmail.template");

            string emailMessage = newUserEmailTemplate
                .Replace("{creator}", creator)
                .Replace("{name}", name)
                .Replace("{userName}", userName)
                .Replace("{roles}", roles)
                .Replace("{company}", company);

            return emailMessage;
        }

        public static string GetForgotPasswordEmail(string recepientName, string userName)
        {
            if (forgotPasswordTemplate == null)
                forgotPasswordTemplate = ReadPhysicalFile("Helpers/Templates/ForgotPassword.template");

            string emailMessage = newUserEmailTemplate
                .Replace("{name}", recepientName)
                .Replace("{userName}", userName);

            return emailMessage;
        }

        public static string GetPasswordResetEmail(string recepientName, string userName)
        {
            if (passwordResetTemplate == null)
                passwordResetTemplate = ReadPhysicalFile("Helpers/Templates/PasswordReset.template");

            string emailMessage = newUserEmailTemplate
                .Replace("{name}", recepientName)
                .Replace("{userName}", userName);

            return emailMessage;
        }


        private static string ReadPhysicalFile(string path)
        {
            if (_webHostEnvironment == null)
                throw new InvalidOperationException($"{nameof(EmailTemplates)} is not initialized");

            IFileInfo fileInfo = _webHostEnvironment.ContentRootFileProvider.GetFileInfo(path);

            if (!fileInfo.Exists)
                throw new FileNotFoundException($"Template file located at \"{path}\" was not found");

            using (var fs = fileInfo.CreateReadStream())
            {
                using (var sr = new StreamReader(fs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
