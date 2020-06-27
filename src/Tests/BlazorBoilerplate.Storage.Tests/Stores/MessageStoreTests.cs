using AutoMapper;
using BlazorBoilerplate.Storage.Stores;
using Moq;
using NUnit.Framework;

namespace BlazorBoilerplate.Storage.Tests.Stores
{
    [TestFixture]
    class MessageStoreTests
    {
        private MessageStore _messageStore;

        private Mock<ApplicationDbContext> _dbContext;
        private Mock<IMapper> _mapper;

        [SetUp]
        public void SetUp()
        {
            _dbContext = new Mock<ApplicationDbContext>();
            _mapper = new Mock<IMapper>();

            _messageStore = new MessageStore(_dbContext.Object, _mapper.Object);
        }

        [Test]
        public void SetupWorked()
        {
            Assert.Pass();
        }
    }
}
