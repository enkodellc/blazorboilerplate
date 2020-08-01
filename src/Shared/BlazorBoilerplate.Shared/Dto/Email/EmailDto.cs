using BlazorBoilerplate.Localization;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Dto.Email
{
    public class EmailDto
    {
        private string _name;

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        public string ToAddress { get; set; }

        public string ToName
        {
          get
          {
            if (string.IsNullOrEmpty(_name))
            {
              return ToAddress;
            }
            return _name;
          }
          set
          {
            _name = value;
          }
        }

        public string FromName { get; set; }

        public string FromAddress { get; set; }

        public string ReplyToAddress { get; set; }

        public string Subject { get; set; }
                
        public string Body { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "FieldRequired")]
        public string TemplateName { get; set; }
    }
}
