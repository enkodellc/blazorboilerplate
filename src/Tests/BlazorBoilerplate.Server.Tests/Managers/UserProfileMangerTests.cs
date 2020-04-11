using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Shared.DataModels;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace BlazorBoilerplate.Server.Tests.Managers
{
    [TestFixture]
    class UserProfileMangerTests
    {
        private UserProfileManager _userProfileManager;

        private Mock<IUserProfileStore> _userProfileStore;
        private Mock<IHttpContextAccessor> _httpContextAccessor;

        [SetUp]
        public void SetUp()
        {
            _userProfileStore = new Mock<IUserProfileStore>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();

            _userProfileManager = new UserProfileManager(_userProfileStore.Object, _httpContextAccessor.Object);
        }

        [Test]
        public void SetupWorked()
        {
            Assert.Pass();
        }
    }
}
