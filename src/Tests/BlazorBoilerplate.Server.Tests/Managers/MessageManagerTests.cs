using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Shared.DataInterfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBoilerplate.Server.Tests.Managers
{
    [TestFixture]
    class MessageManagerTests
    {
        private MessageManager _messageManager;

        private Mock<IMessageStore> _messageStore;
        private Mock<ILogger<MessageManager>> _logger;

        [SetUp]
        public void SetUp()
        {
            _messageStore = new Mock<IMessageStore>();
            _logger = new Mock<ILogger<MessageManager>>();

            _messageManager = new MessageManager(_messageStore.Object, _logger.Object);
        }

        [Test]
        public void SetupWorked()
        {
            Assert.Pass();
        }
    }
}
