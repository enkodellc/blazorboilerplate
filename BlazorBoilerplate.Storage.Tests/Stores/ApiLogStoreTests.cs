using AutoMapper;
using BlazorBoilerplate.Storage.Stores;
using Moq;
using NUnit.Framework;

namespace BlazorBoilerplate.Storage.Tests.Stores
{
    [TestFixture]
    class ApiLogStoreTests
    {
        private ApiLogStore _apiLogStore;

        private Mock<IApplicationDbContext> _dbContext;
        private Mock<IMapper> _mapper;

        [SetUp]
        public void SetUp()
        {
            _dbContext = new Mock<IApplicationDbContext>();
            _mapper = new Mock<IMapper>();

            _apiLogStore = new ApiLogStore(_dbContext.Object, _mapper.Object);
        }

        [Test]
        public void SetupWorked()
        {
            Assert.Pass();
        }
    }
}
