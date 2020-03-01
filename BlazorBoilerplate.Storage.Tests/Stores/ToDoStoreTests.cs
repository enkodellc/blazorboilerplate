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
    class ToDoStoreTests
    {
        private ToDoStore _toDoStore;

        private Mock<IApplicationDbContext> _dbContext;
        private Mock<IMapper> _mapper;

        [SetUp]
        public void SetUp()
        {
            _dbContext = new Mock<IApplicationDbContext>();
            _mapper = new Mock<IMapper>();

            _toDoStore = new ToDoStore(_dbContext.Object, _mapper.Object);
        }

        [Test]
        public void SetupWorked()
        {
            Assert.Pass();
        }
    }
}
