using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SampleService.Interfaces;
using SampleService.Services;
using SharedLibrary.Handlers.Interfaces;
using SharedLibrary.Models;
using Xunit;
namespace UnitTesting
{
    public class SampleStreamPublisServiceUnitTests
    {
        private readonly Mock<IProducerBuilderHandler> _producerHandler;
        private readonly Mock<ILogger<PublishService>> _logger;

        public SampleStreamPublisServiceUnitTests()
        {
            _producerHandler = new Mock<IProducerBuilderHandler>();
            _logger = new Mock<ILogger<PublishService>>();
        }

        [Fact]
        public async Task Publis_UnitTest_Less100()
        {
            _producerHandler.Setup(x => x.ProduceAsync()).Verifiable();
            _producerHandler.Setup(x => x.BootstrapServers).Returns("BootstrapServers");
            _producerHandler.Setup(x => x.Topic).Returns("topic1");

            _producerHandler.Raise(x => x.ProcessCompleted += Producer_ProcessCompleted);

            var pubishService = new PublishService(_logger.Object, _producerHandler.Object);
     
            string[] lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\data.txt");
          
            var record = JsonConvert.DeserializeObject<TwitterRecord>(lines[0]);

            await pubishService.Publish(record, CancellationToken.None);
            _producerHandler.Verify(x => x.ProduceAsync(), Times.Never);
        }

        public async Task Publis_UnitTest_99records()
        {
            _producerHandler.Setup(x => x.ProduceAsync()).Verifiable();
            _producerHandler.Setup(x => x.BootstrapServers).Returns("BootstrapServers");
            _producerHandler.Setup(x => x.Topic).Returns("topic1");

            _producerHandler.Raise(x => x.ProcessCompleted += Producer_ProcessCompleted);

            var pubishService = new PublishService(_logger.Object, _producerHandler.Object);



            string[] lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\tweet99records.txt");

            List<TwitterRecord> records = new List<TwitterRecord>();

            foreach (string line in lines)
            {
                records.Add(JsonConvert.DeserializeObject<TwitterRecord>(line));
            }
          
            pubishService.BulkRecords = records;

            var record = JsonConvert.DeserializeObject<TwitterRecord>(lines[0]);
            await pubishService.Publish(record, CancellationToken.None);
            _producerHandler.Verify(x => x.ProduceAsync(), Times.Once);
        }

        private void Producer_ProcessCompleted(string data)
        {
            Console.WriteLine(data);
        }

    }
}
