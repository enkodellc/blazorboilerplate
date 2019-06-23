using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BlazorBoilerplate.Shared
{
    public class EmailParameters
    {
        [Required]
        public string ToAddress { get; set; }
 
        public string ToName { get; set; }

        public string FromName { get; set; }

        public string FromAddress { get; set; }

        public string ReplyToAddress { get; set; }

        [Required]
        public string Subject { get; set; }
        
        [Required]
        public string Body { get; set; }
          
        public int TemplateId { get; set; }
    }
}
