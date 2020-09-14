using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Shared.SqlLocalizer;
using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Shared.Models.Account;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Encodings.Web;

namespace BlazorBoilerplate.Server.Tests.Managers
{
    [TestFixture]
    class AccountMangerTests
    {
        private AccountManager _accountManager;

        private Mock<IDatabaseInitializer> _databaseInitializer;
        private Mock<UserManager<ApplicationUser>> _userManager;
        private Mock<SignInManager<ApplicationUser>> _signInManager;
        private Mock<ILogger<AccountManager>> _logger;
        private Mock<RoleManager<ApplicationRole>> _roleManager;
        private Mock<IEmailManager> _emailManager;
        private Mock<IClientStore> _clientStore;
        private Mock<IConfiguration> _configuration;
        private Mock<IIdentityServerInteractionService> _interaction;
        private Mock<IAuthenticationSchemeProvider> _schemeProvider;
        private Mock<UrlEncoder> _urlEncoder;
        private Mock<IEventService> _events;
        private Mock<IStringLocalizer<Global>> _l;


        [SetUp]
        public void SetUp()
        {
            // Break out some of this to a base class that can be shared by tests. This would suck to set up in all test blocks. 
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var roleStore = new Mock<IRoleStore<ApplicationRole>>();

            var roles = new List<IRoleValidator<ApplicationRole>>();
            roles.Add(new RoleValidator<ApplicationRole>());

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            _databaseInitializer = new Mock<IDatabaseInitializer>(null, null, null, null, null, null, null);
            _userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
            _signInManager = new Mock<SignInManager<ApplicationUser>>(_userManager.Object, contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);
            _logger = new Mock<ILogger<AccountManager>>();
            _roleManager = new Mock<RoleManager<ApplicationRole>>(roleStore.Object, roles, new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null);
            _emailManager = new Mock<IEmailManager>();
            _configuration = new Mock<IConfiguration>();
            _urlEncoder = new Mock<UrlEncoder>();
            _events = new Mock<IEventService>();
            _l = new Mock<IStringLocalizer<Global>>();

            _accountManager = new AccountManager(_databaseInitializer.Object,
                _userManager.Object, 
                _signInManager.Object, 
                _logger.Object, 
                _roleManager.Object, 
                _emailManager.Object,
                _clientStore.Object,
                _configuration.Object,
                _interaction.Object,
                _schemeProvider.Object,
                _urlEncoder.Object,
                _events.Object,
                _l.Object);
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

            var confirmEmailViewModel = new ConfirmEmailViewModel();
            confirmEmailViewModel.Token = null;
            confirmEmailViewModel.UserId = null;


            // Act

            var response = await _accountManager
                .ConfirmEmail(confirmEmailViewModel);


            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(404));
        }
    }
}
