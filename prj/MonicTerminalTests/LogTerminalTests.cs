
using System;
using System.Linq;
using MonikTerminal;
using MonikTerminal.Enums;
using MonikTerminal.Interfaces;
using MonikTerminal.ModelsApi;
using Moq;
using NUnit.Framework;

namespace MonicTerminalTests
{
    [TestFixture]
    public class LogTerminalTests
    {
        private LogTerminal terminal;

        private Mock<IMonikService> monicMock  = new Mock<IMonikService>();
        private IConfig             config     = new Config();
        private Mock<ISourcesCache> cacheMock  = new Mock<ISourcesCache>();

        [OneTimeSetUp]
        public void Inicialise()
        {
            terminal = new LogTerminal(monicMock.Object, config, cacheMock.Object);
        }

        [Test]
        public void TestGrouping_Empty_Returns_Empty()
        {
            var logs = new ELog_[0];
            var rez = terminal.GroupDuplicatingLogs(logs);

            Assert.That(rez.Length == 0);
        }

        [Test]
        public void TestGrouping_NoDoubling_Returns_TheSameLogs()
        {
            var curTime = DateTime.Now;

            var logs = new[]
            {
                new ELog_()
                {
                    Created = curTime,
                    Body = "testBody1",
                    InstanceID = 1,
                    Level = (byte)LevelType.Application,
                    Severity = (byte)SeverityCutoffType.Info
                }, 
                new ELog_()
                {
                    Created = curTime.AddSeconds(config.RefreshPeriod+1),
                    Body = "testBody2",
                    InstanceID = 2,
                    Level = (byte)LevelType.Logic,
                    Severity = (byte)SeverityCutoffType.Error
                }, 
            };
            var rez = terminal.GroupDuplicatingLogs(logs);

            Assert.That(rez, Is.EquivalentTo(logs));
        }

        [Test]
        public void TestGrouping_DoublingInOnlyOneField_Returns_TheSameLogs()
        {
            var curTime = DateTime.Now;

            var logs = new[]
            {
                new ELog_()
                {
                    Created = curTime,
                    Body = "testBody1",
                    InstanceID = 1,
                    Level = (byte)LevelType.Application,
                    Severity = (byte)SeverityCutoffType.Info
                }, 
                new ELog_()
                {
                    Created = curTime.AddSeconds(config.RefreshPeriod+1),
                    Body = "testBody2",
                    InstanceID = 2,
                    Level = (byte)LevelType.Application,
                    Severity = (byte)SeverityCutoffType.Error
                }, 
            };
            var rez = terminal.GroupDuplicatingLogs(logs);

            Assert.That(rez, Is.EquivalentTo(logs));
        }

        [Test]
        public void TestGrouping_DoublingWithoutTime_Returns_TheSameLogs()
        {
            var curTime = DateTime.Now;

            var logs = new[]
            {
                new ELog_()
                {
                    Created = curTime,
                    Body = "testBody1",
                    InstanceID = 1,
                    Level = (byte)LevelType.Application,
                    Severity = (byte)SeverityCutoffType.Info
                }, 
                new ELog_()
                {
                    Created = curTime.AddSeconds(config.RefreshPeriod+1),
                    Body = "testBody1",
                    InstanceID = 1,
                    Level = (byte)LevelType.Application,
                    Severity = (byte)SeverityCutoffType.Info
                }, 
            };
            var rez = terminal.GroupDuplicatingLogs(logs);

            Assert.That(rez, Is.EquivalentTo(logs));
        }

        [Test]
        public void TestGrouping_Doubling_Returns_First()
        {
            var curTime = DateTime.Now;

            var logs = new[]
            {
                new ELog_()
                {
                    Created = curTime,
                    Body = "testBody1",
                    InstanceID = 1,
                    Level = (byte)LevelType.Application,
                    Severity = (byte)SeverityCutoffType.Info
                }, 
                new ELog_()
                {
                    Created = curTime.AddSeconds(config.RefreshPeriod-1),
                    Body = "testBody1",
                    InstanceID = 1,
                    Level = (byte)LevelType.Application,
                    Severity = (byte)SeverityCutoffType.Info
                }, 
            };
            var rez = terminal.GroupDuplicatingLogs(logs);
            
            Assert.That(rez, Is.EquivalentTo(new[] { logs.First() }));
            Assert.That(rez.First().Doubled);
        }

        [Test]
        public void TestGrouping_Doubling_and_NotDoubledTime_AndNotDoubling_Returns_Firsts()
        {
            var curTime = DateTime.Now;

            var logs = new[]
            {
                new ELog_()
                {
                    Created = curTime,
                    Body = "testBody1",
                    InstanceID = 1,
                    Level = (byte)LevelType.Application,
                    Severity = (byte)SeverityCutoffType.Info
                }, 
                new ELog_()
                {
                    Created = curTime.AddSeconds(config.RefreshPeriod-1),
                    Body = "testBody1",
                    InstanceID = 1,
                    Level = (byte)LevelType.Application,
                    Severity = (byte)SeverityCutoffType.Info
                }, 
                new ELog_()
                {
                    Created = curTime.AddSeconds(config.RefreshPeriod+1),
                    Body = "testBody1",
                    InstanceID = 1,
                    Level = (byte)LevelType.Application,
                    Severity = (byte)SeverityCutoffType.Info
                },
                new ELog_()
                {
                    Created = curTime.AddSeconds(config.RefreshPeriod+1),
                    Body = "testBody2",
                    InstanceID = 2,
                    Level = (byte)LevelType.Logic,
                    Severity = (byte)SeverityCutoffType.Error
                },
            };
            var rez = terminal.GroupDuplicatingLogs(logs);
            
            Assert.That(rez, Is.EquivalentTo(new[] { logs[0], logs[2], logs[3] }));
            Assert.That(rez[0].Doubled);
            Assert.That(!rez[1].Doubled);
            Assert.That(!rez[2].Doubled);
        }
    }
}
