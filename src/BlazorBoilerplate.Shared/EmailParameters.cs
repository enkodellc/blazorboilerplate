using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared
{
    public class EmailParameters
    {
        private string _name;

        [Required]
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

        //[Required]
        public string Subject { get; set; }

        //[Required]
        public string Body { get; set; }

        [Required]
        public string TemplateName { get; set; }
    }
}
