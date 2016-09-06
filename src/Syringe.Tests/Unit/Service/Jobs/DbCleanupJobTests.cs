﻿using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using Syringe.Core.Configuration;
using Syringe.Core.Tests.Results.Repositories;
using Syringe.Service.Jobs;

namespace Syringe.Tests.Unit.Service.Jobs
{
    [TestFixture]
    public class DbCleanupJobTests
    {
        private Mock<IConfiguration> _configurationMock;
        private Mock<ITestFileResultRepository> _repositoryMock;
        private int _callbackCount;
        private DbCleanupJob _job;

        [SetUp]
        public void Setup()
        {
            _callbackCount = 0;
            _configurationMock = new Mock<IConfiguration>();
            _repositoryMock = new Mock<ITestFileResultRepository>();
            _job = new DbCleanupJob(_configurationMock.Object, _repositoryMock.Object);
        }

        [Test]
        public void should_clear_results_before_now()
        {
            // given
            const int expectedDaysOfRetention = 66;
            _configurationMock
                .Setup(x => x.DaysOfDataRetention)
                .Returns(expectedDaysOfRetention);

            // when
            _job.Cleanup(null);

            // then
            _repositoryMock
                .Verify(x => x.DeleteBeforeDate(DateTime.Today.AddDays(-expectedDaysOfRetention)), Times.Once);
        }

        [Test]
        public void shold_execute_given_callback_via_timer_and_then_stop()
        {
            // given
            _configurationMock
                .Setup(x => x.CleanupSchedule)
                .Returns(new TimeSpan(0, 0, 0, 0, 10)); // 10 milli

            // when
            _job.Start(DummyCallback);

            // then
            Thread.Sleep(new TimeSpan(0, 0, 0, 0, 50)); // 50 milli
            Assert.That(_callbackCount, Is.GreaterThanOrEqualTo(3));

            _job.Stop();
            int localCallbackStore = _callbackCount;
            Thread.Sleep(new TimeSpan(0, 0, 0, 0, 50)); // 50 milli
            Assert.That(_callbackCount, Is.EqualTo(localCallbackStore));
        }

        private void DummyCallback(object guff)
        {
            _callbackCount++;
        }
    }
}