using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Email;

namespace BlazorBoilerplate.Server.Helpers
{
    public static class EmailTemplates
    {
        static IWebHostEnvironment _webHostEnvironment;
        static string testEmailTemplate;
        static string plainTextTestEmailTemplate;
        static string newUserEmailTemplate;
        static string newUserConfirmationEmailTemplate;
        static string newUserNotificationEmailTemplate;
        static string passwordResetTemplate;
        static string forgotPasswordTemplate;

        public static void Initialize(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public static EmailMessageDto BuildTestEmail(this EmailMessageDto emailMessage)
        {           
            if (testEmailTemplate == null)
                testEmailTemplate = ReadPhysicalFile("Helpers/Templates/TestEmail.template");

              emailMessage.Body = testEmailTemplate
                .Replace("{user}", emailMessage.ToAddresses[0].Name)
                .Replace("{testDate}", DateTime.Now.ToString());

              emailMessage.Subject = string.Format("Hello {0} from Blazor Boilerplate", emailMessage.ToAddresses[0].Name);

          return emailMessage;
        }
        public static EmailMessageDto GetPlainTextTestEmail(this EmailMessageDto emailMessage, DateTime date)
        {
            if (plainTextTestEmailTemplate == null)
                plainTextTestEmailTemplate = ReadPhysicalFile("Helpers/Templates/PlainTextTestEmail.template");

            emailMessage.Body = plainTextTestEmailTemplate
                .Replace("{date}", date.ToString());

            emailMessage.IsHtml = false;

            return emailMessage;
        }        
        public static EmailMessageDto BuildNewUserConfirmationEmail(this EmailMessageDto emailMessage, string recepientName, string userName, string callbackUrl, string userId, string token)
        {
            if (newUserConfirmationEmailTemplate == null)
                newUserConfirmationEmailTemplate = ReadPhysicalFile("Helpers/Templates/NewUserConfirmationEmail.template");

            emailMessage.Body = newUserConfirmationEmailTemplate
                //.Replace("{name}", recepientName) // Uncomment if you want to add name to the registration form
                .Replace("{userName}", userName)
                .Replace("{callbackUrl}", callbackUrl)
                .Replace("{userId}", userId)
                .Replace("{token}", token);

            emailMessage.Subject = string.Format("Welcome {0} to Blazor Boilerplate", recepientName);

            return emailMessage;
        }
        public static EmailMessageDto BuildNewUserEmail(this EmailMessageDto emailMessage, string fullName, string userName, string emailAddress, string password)
        {
            if (newUserEmailTemplate == null)
                newUserEmailTemplate = ReadPhysicalFile("Helpers/Templates/NewUserEmail.template");

            emailMessage.Body = newUserEmailTemplate
                //.Replace("{fullName}", fullName) // Uncomment if you want to add name to the registration form has First / Last Name
                .Replace("{fullName}", userName) //Comment out if you want have First / Last Name in registration form.
                .Replace("{userName}", userName)
                .Replace("{email}", emailAddress)
                .Replace("{password}", password);

            emailMessage.Subject = string.Format("Welcome {0} to Blazor Boilerplate", fullName);

            return emailMessage;
        }
        public static EmailMessageDto BuilNewUserNotificationEmail(this EmailMessageDto emailMessage, string creator, string name, string userName, string company, string roles)
        {
            //placeholder not actually implemented
            if (newUserNotificationEmailTemplate == null)
                newUserNotificationEmailTemplate = ReadPhysicalFile("Helpers/Templates/NewUserEmail.template");

            emailMessage.Body = newUserNotificationEmailTemplate
                .Replace("{creator}", creator)
                .Replace("{name}", name)
                .Replace("{userName}", userName)
                .Replace("{roles}", roles)
                .Replace("{company}", company);

            emailMessage.Subject = string.Format("A new user [{0}] has registered on Blazor Boilerplate", userName);

            return emailMessage;
        }
        public static EmailMessageDto BuildForgotPasswordEmail(this EmailMessageDto emailMessage, string name, string callbackUrl, string token)
        {
            if (forgotPasswordTemplate == null)
                forgotPasswordTemplate = ReadPhysicalFile("Helpers/Templates/ForgotPassword.template");

            emailMessage.Body = forgotPasswordTemplate
                .Replace("{name}", name)
                .Replace("{token}", token)
                .Replace("{callbackUrl}", callbackUrl);

            emailMessage.Subject = string.Format("Blazor Boilerplate Forgot your Passord? [{0}]", name);

            return emailMessage;
        }
        public static EmailMessageDto BuildPasswordResetEmail(this EmailMessageDto emailMessage, string userName)
        {
            if (passwordResetTemplate == null)
                passwordResetTemplate = ReadPhysicalFile("Helpers/Templates/PasswordReset.template");

            emailMessage.Body = passwordResetTemplate
                .Replace("{userName}", userName);

            emailMessage.Subject = string.Format("Blazor Boilerplate Password Reset for {0}", userName);

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
