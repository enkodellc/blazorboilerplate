using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Tests.Managers
{
    [TestFixture]
    class AccountMangerTests
    {
        private AccountManager _accountManager;

        private Mock<UserManager<ApplicationUser>> _userManager;
        private Mock<SignInManager<ApplicationUser>> _signInManager;
        private Mock<ILogger<AccountManager>> _logger;
        private Mock<RoleManager<IdentityRole<Guid>>> _roleManager;
        private Mock<IEmailManager> _emailManager;
        private Mock<IUserProfileStore> _userProfileStore;
        private Mock<IConfiguration> _configuration;


        [SetUp]
        public void SetUp()
        {
            // Break out some of this to a base class that can be shared by tests. This would suck to set up in all test blocks. 
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var roleStore = new Mock<IRoleStore<IdentityRole<Guid>>>();

            var roles = new List<IRoleValidator<IdentityRole<Guid>>>();
            roles.Add(new RoleValidator<IdentityRole<Guid>>());

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            _userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
            _signInManager = new Mock<SignInManager<ApplicationUser>>(_userManager.Object, contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);
            _logger = new Mock<ILogger<AccountManager>>();
            _roleManager = new Mock<RoleManager<IdentityRole<Guid>>>(roleStore.Object, roles, new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null);
            _emailManager = new Mock<IEmailManager>();
            _userProfileStore = new Mock<IUserProfileStore>();
            _configuration = new Mock<IConfiguration>();

            _accountManager = new AccountManager(_userManager.Object, 
                _signInManager.Object, 
                _logger.Object, 
                _roleManager.Object, 
                _emailManager.Object, 
                _userProfileStore.Object, 
                _configuration.Object);
        }

        [Test]
        public void SetupWorked()
        {
            Assert.Pass();
        }

        [Test]
        public async Task ConfirmEmail_WithInvaildParameters_Returns404()
        {
            // Arange 

            var confirmEmailDto = new ConfirmEmailDto();
            confirmEmailDto.Token = null;
            confirmEmailDto.UserId = null;


            // Act

            var response = await _accountManager
                .ConfirmEmail(confirmEmailDto);


            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(404));
        }
    }
}
