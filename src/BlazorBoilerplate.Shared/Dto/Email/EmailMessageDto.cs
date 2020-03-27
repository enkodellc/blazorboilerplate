using System.Collections.Generic;

namespace BlazorBoilerplate.Shared.Dto.Email
{
    public class EmailMessageDto
    {
        public EmailMessageDto()
        {
            ToAddresses = new List<EmailAddressDto>();
            FromAddresses = new List<EmailAddressDto>();
            CcAddresses = new List<EmailAddressDto>();
            BccAddresses = new List<EmailAddressDto>();
        }

        public List<EmailAddressDto> ToAddresses { get; set; }
        public List<EmailAddressDto> FromAddresses { get; set; }
        public List<EmailAddressDto> BccAddresses { get; set; }
        public List<EmailAddressDto> CcAddresses { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; } = true;
    }
}
