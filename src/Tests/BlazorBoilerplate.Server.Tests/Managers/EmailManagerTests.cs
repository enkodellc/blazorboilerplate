using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace BlazorBoilerplate.Server.Tests.Managers
{
    [TestFixture]
    class EmailManagerTests
    {
        private EmailManager _emailManager;
        private Mock<ITenantSettings<EmailConfiguration>> _emailConfiguration;
        private Mock<ILogger<EmailManager>> _logger;

        [SetUp]
        public void SetUp()
        {
            _emailConfiguration = new Mock<ITenantSettings<EmailConfiguration>>();
            _logger = new Mock<ILogger<EmailManager>>();

            _emailManager = new EmailManager(_emailConfiguration.Object, _logger.Object);
        }

        [Test]
        public void SetupWorked()
        {
            Assert.Pass();
        }
    }
}
