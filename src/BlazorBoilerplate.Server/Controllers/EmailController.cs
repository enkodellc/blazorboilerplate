using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Server.Helpers;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class EmailController : ControllerBase
    {
        // Logger instance
        ILogger<EmailController> _logger;
        IEmailService _emailService;

        public EmailController(IEmailService emailService, ILogger<EmailController> logger)
        {
            _logger = logger;
            _emailService = emailService;
        }

        [HttpPost("Send")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Send(EmailParameters parameters)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(state => state.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault());

            var email = new EmailMessage();

            email.ToAddresses.Add(new EmailAddress(parameters.ToName, parameters.ToAddress));

            //This forces all emails from the API to use the Test template to prevent spam
            parameters.TemplateName = "Test";

            //Send a Template email or a custom one since it is hardcoded to Test template it will not do a custom email.
            if (!string.IsNullOrEmpty(parameters.TemplateName))
            {
                switch (parameters.TemplateName)
                {
                    case "Test":
                        email = EmailTemplates.BuildTestEmail(email); //example of email Template usage
                        break;
                    default:
                        break;
                }
            }
            else
            {
                email.Subject = parameters.Subject;
                email.Body = parameters.Body;
            }

            //Add a new From Address if you so choose, default is set in appsettings.json
            //email.FromAddresses.Add(new EmailAddress("New From Name", "email@domain.com"));
            _logger.LogInformation("Test Email: {0}", email.Subject);

            await _emailService.SendEmailAsync(email);

            return Ok(new { success = "true" });
        }
    }
}
