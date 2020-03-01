using AutoMapper;
using BlazorBoilerplate.Storage.Stores;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBoilerplate.Storage.Tests.Stores
{
    [TestFixture]
    class UserProfileStoreTests
    {
        private UserProfileStore _userProfileStore;

        private Mock<IApplicationDbContext> _dbContext;

        [SetUp]
        public void SetUp()
        {
            _dbContext = new Mock<IApplicationDbContext>();

            _userProfileStore = new UserProfileStore(_dbContext.Object);
        }

        [Test]
        public void SetUpWorked()
        {
            Assert.Pass();
        }

    }
}
