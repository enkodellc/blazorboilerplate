using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Shared.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBoilerplate.Server.Tests.Managers
{
    [TestFixture]
    class ExternalAuthManagerTests
    {
        private ExternalAuthManager _externalAuthManager;

        private Mock<IAccountManager> _accountManager;
        private Mock<UserManager<ApplicationUser>> _userManager;
        private Mock<SignInManager<ApplicationUser>> _signInManager;
        private Mock<ILogger<ExternalAuthManager>> _logger;
        private Mock<IConfiguration> _configuration;



        [SetUp]
        public void SetUp()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            _accountManager = new Mock<IAccountManager>();
            _userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
            _signInManager = new Mock<SignInManager<ApplicationUser>>(_userManager.Object, contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);
            _logger = new Mock<ILogger<ExternalAuthManager>>();
            _configuration = new Mock<IConfiguration>();

            _externalAuthManager = new ExternalAuthManager(_accountManager.Object, _userManager.Object, _signInManager.Object, _logger.Object, _configuration.Object);


        }

        [Test]
        public void SetupWorked()
        {
            Assert.Pass();
        }
    }
}
