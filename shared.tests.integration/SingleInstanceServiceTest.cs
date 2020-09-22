using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace shared.tests.integration
{
    [Category("Integration")]
    [TestFixture]
    public class SingleInstanceServiceTest
    {
        private SingleInstanceService _singleInstanceService;

        [SetUp]
        public void Setup()
        {
            _singleInstanceService = new SingleInstanceService();
        }

        [Test]
        public async Task Start_NoOtherInstance_ReturnsTrue()
        {
            var isSingleInstance = await _singleInstanceService.Start();

            Assert.IsTrue(isSingleInstance);

            _singleInstanceService.Stop();
        }

        [Test]
        public async Task Start_AnotherInstanceIsRunning_ReturnsFalse()
        {
            await _singleInstanceService.Start();
            var singleInstanceService2 = new SingleInstanceService();

            var isSingleInstance = await singleInstanceService2.Start();
            
            Assert.IsFalse(isSingleInstance);

            singleInstanceService2.Stop();
            _singleInstanceService.Stop();
        }

        [Test]
        public async Task SingleFirstInstance_AnotherInstanceIsRunning_MessageIsReceived()
        {
            await _singleInstanceService.Start();
            var commands = new List<string>();
            _singleInstanceService.CommandsReceived += (o, e) => { commands.AddRange(e); };
            var singleInstanceService2 = new SingleInstanceService();

            await singleInstanceService2.Start();

            await singleInstanceService2.SignalFirstInstance(new List<string> {"Hello!"});

            await Task.Delay(1000);

            Assert.IsNotEmpty(commands);

            singleInstanceService2.Stop();
            _singleInstanceService.Stop();
        }

        [Test]
        public async Task SingleFirstInstance_SignalOtherInstanceTwoTimes_MessageIsReceivedTwoTimes()
        {
            await _singleInstanceService.Start();
            var commands = new List<string>();
            _singleInstanceService.CommandsReceived += (o, e) => { commands.AddRange(e); };

            var singleInstanceService2 = new SingleInstanceService();

            await singleInstanceService2.Start();

            await singleInstanceService2.SignalFirstInstance(new List<string>() { "Hello!" });
            await singleInstanceService2.SignalFirstInstance(new List<string>() { "Hello!" });

            await Task.Delay(1000);

            Assert.That(commands.Count == 2);

            singleInstanceService2.Stop();
            _singleInstanceService.Stop();
        }
    }
}