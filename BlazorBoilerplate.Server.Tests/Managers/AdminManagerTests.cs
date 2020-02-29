using BlazorBoilerplate.Shared.DataModels;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBoilerplate.Server.Tests.Managers
{
    [TestFixture]
    class AdminManagerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManager;
        private Mock<RoleManager<IdentityRole<Guid>>> _roleManager;

        [SetUp]
        public void SetUp()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var roleStore = new Mock<IRoleStore<IdentityRole<Guid>>>();

            var roles = new List<IRoleValidator<IdentityRole<Guid>>>
            {
                new RoleValidator<IdentityRole<Guid>>()
            };

            _userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
            _roleManager = new Mock<RoleManager<IdentityRole<Guid>>>(roleStore.Object, roles, new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null);
        }

        [Test]
        public void SetupWorked()
        {
            Assert.Pass();
        }
    }
}
