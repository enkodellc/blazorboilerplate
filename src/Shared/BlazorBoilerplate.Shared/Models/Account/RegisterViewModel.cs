using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Models.Account
{
    public class RegisterViewModel : LoginInputModel
    {
        public bool UserNameEqualsEmail { get; set; }
        private string email;

        [DataType(DataType.EmailAddress)]
        public string Email
        {
            get => email;
            set
            {
                email = value;

                if (UserNameEqualsEmail)
                    UserName = email;
            }
        }
        public string PasswordConfirm { get; set; }
    }
}
