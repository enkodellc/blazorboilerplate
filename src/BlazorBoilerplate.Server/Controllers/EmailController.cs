using BlazorBoilerplate.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Models;
using Microsoft.Extensions.Logging;
using BlazorBoilerplate.Server.Services;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class EmailController : Controller
    {
        // Logger instance
        ILogger<SampleDataController> _logger;
        IEmailService _emailService;

        public EmailController(IEmailService emailService, ILogger<SampleDataController> logger)
        {
            _logger = logger;
            _emailService = emailService;
        }


        [HttpPost]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(string))]
        public async Task<IActionResult> EmailTest(EmailParameters parameters)
        {
            //Todo fix these parameters
            parameters.Subject = "test";
            parameters.Body = "my email";
            parameters.ToAddress = "support@blazorboilerplate.com";

            var email = new EmailMessage();
            email.ToAddresses.Add(new EmailAddress(parameters.ToAddress, parameters.ToAddress));
            email.Subject = parameters.Subject;
            email.Body = parameters.Body;
            email.FromAddresses.Add(new EmailAddress("support@blazorboilerplate.com", "support@blazorboilerplate.com"));

            _logger.LogInformation("Test Email: {0}", email.Subject );

            await _emailService.SendEmailAsync(email);

            return Ok(new {success="true"});
        }
    }
}
