using SharedLibrary.Handlers.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using ProcessService.Interfaces;
using ProcessService.Services;
using SharedLibrary.Models;
using Xunit;

namespace UnitTesting
{
    public class UpdateDataServiceUnitTests
    {
        private readonly Mock<IProducerBuilderHandler> _producerHandler;
        private readonly Mock<ILogger<DataProcessService>> _logger;
        public UpdateDataServiceUnitTests()
        {
            _producerHandler = new Mock<IProducerBuilderHandler>();
            _logger = new Mock<ILogger<DataProcessService>>();
        }

        [Fact]
        public async Task UpdateData_UnitTest()
        {
            TweetResponse tweetResponse = new TweetResponse();
            tweetResponse.TweentTotalCount = 100;
            tweetResponse.HashTags = new Dictionary<string, int>()
            {
                { "tag1", 100},
                { "tag2", 90},
                { "tag3", 80},
                { "tag4", 70},
                { "tag5", 60},
                { "tag6", 50},
                { "tag7", 40},
                { "tag8", 30},
                { "tag9", 20},
                { "tag10", 10},

            };

            _producerHandler.Setup(x => x.ProduceAsync()).Verifiable();
            _producerHandler.Setup(x => x.BootstrapServers).Returns("BootstrapServers");
            _producerHandler.Setup(x => x.Topic).Returns("topic2");
            _producerHandler.Setup(x => x.Data).Returns(JsonConvert.SerializeObject(tweetResponse));
            _producerHandler.Raise(x => x.ProcessCompleted += Producer_ProcessCompleted);
            List<TwitterRecord> records = new List<TwitterRecord>();
            string[] lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\data.txt");
            foreach(string line in lines)
            {
                records.Add(JsonConvert.DeserializeObject<TwitterRecord>(line));
            }
            var _updateDataService = new UpdateDataService(_logger.Object, _producerHandler.Object);
            await _updateDataService.UpdateData(records);

            _producerHandler.Verify(x => x.ProduceAsync(), Times.Once);
        }


        private void Producer_ProcessCompleted(string data)
        {
            Console.WriteLine(data);
        }

    }
}
