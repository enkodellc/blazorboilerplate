using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using BlazorBoilerplate.Server.Models;

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

        public static EmailMessage BuildTestEmail(EmailMessage emailMessage, string recepientName)
        {           
            if (testEmailTemplate == null)
                testEmailTemplate = ReadPhysicalFile("Helpers/Templates/TestEmail.template");

              emailMessage.Body = testEmailTemplate
                .Replace("{user}", recepientName)
                .Replace("{testDate}", DateTime.Now.ToString());

              emailMessage.Subject = string.Format("Hello {0} from Blazor Boilerplate", recepientName);

          return emailMessage;
        }

        public static EmailMessage GetPlainTextTestEmail(EmailMessage emailMessage, DateTime date)
        {
            if (plainTextTestEmailTemplate == null)
                plainTextTestEmailTemplate = ReadPhysicalFile("Helpers/Templates/PlainTextTestEmail.template");

            emailMessage.Body = plainTextTestEmailTemplate
                .Replace("{date}", date.ToString());

            emailMessage.IsHtml = false;

            return emailMessage;
        }

        public static EmailMessage BuildNewUserEmail(EmailMessage emailMessage, string recepientName, string userName, string password)
        {
            if (newUserEmailTemplate == null)
                newUserEmailTemplate = ReadPhysicalFile("Helpers/Templates/NewUserEmail.template");

            emailMessage.Body = newUserEmailTemplate
                //.Replace("{name}", recepientName) //use if you want to add name to the registration form
                .Replace("{userName}", userName)
                .Replace("{password}", password);

            emailMessage.Subject = string.Format("Welcome {0} to Blazor Boilerplate", recepientName);

            return emailMessage;
        }

        public static EmailMessage BuilNewUserNotificationEmail(EmailMessage emailMessage, string creator, string name, string userName, string company, string roles)
        {
            //placeholder not actually implemented
            if (newUserNotificationEmailTemplate == null)
                newUserNotificationEmailTemplate = ReadPhysicalFile("Helpers/Templates/NewUserEmail.template");

            emailMessage.Body = newUserEmailTemplate
                .Replace("{creator}", creator)
                .Replace("{name}", name)
                .Replace("{userName}", userName)
                .Replace("{roles}", roles)
                .Replace("{company}", company);

            emailMessage.Subject = string.Format("A new user [{0}] has registered on Blazor Boilerplate", userName);

            return emailMessage;
        }

        public static EmailMessage BuildForgotPasswordEmail(EmailMessage emailMessage, string recepientName, string userName)
        {
            //placeholder not actually implemented
            if (forgotPasswordTemplate == null)
                forgotPasswordTemplate = ReadPhysicalFile("Helpers/Templates/ForgotPassword.template");

            emailMessage.Body = newUserEmailTemplate
                .Replace("{name}", recepientName)
                .Replace("{userName}", userName);

            emailMessage.Subject = string.Format("Forgot your Passord? [{0}]", userName);

            return emailMessage;
        }

        public static EmailMessage BuildPasswordResetEmail(EmailMessage emailMessage, string recepientName, string userName)
        {
            //placeholder not actually implemented
            if (passwordResetTemplate == null)
                passwordResetTemplate = ReadPhysicalFile("Helpers/Templates/PasswordReset.template");

            emailMessage.Body = newUserEmailTemplate
                .Replace("{name}", recepientName)
                .Replace("{userName}", userName);

            emailMessage.Subject = string.Format("Blazor Boilerplate Password Reset for {0}", recepientName);

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
