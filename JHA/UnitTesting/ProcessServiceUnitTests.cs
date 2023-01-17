using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using ProcessService.Services;
using SampleService.Interfaces;
using SampleService.Services;
using SharedLibrary.Handlers.Interfaces;
using SharedLibrary.Models;
using Xunit;


namespace UnitTesting
{
    public class ProcessServiceUnitTests
    {
        private Mock<ILogger<DataProcessService>> _loggerMock;
        private readonly IConfiguration configuration;
        private readonly Mock<IConsumerBuilderHandler> _consumberHandler;
        private readonly Mock<IProducerBuilderHandler> _producerHandler;
       
        public ProcessServiceUnitTests()
        {
            configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile(@"appsettings.json", false, false)
              .AddEnvironmentVariables()
              .Build();

            _loggerMock = new Mock<ILogger<DataProcessService>>();
            _consumberHandler = new Mock<IConsumerBuilderHandler>();
            _producerHandler = new Mock<IProducerBuilderHandler>();
        }

        [Fact]
        public void ProcessData_Successfully()
        {
            _consumberHandler.Setup(x => x.Subscribe()).Verifiable();
            _consumberHandler.Setup(x => x.BootstrapServers).Returns("BootstrapServers");
            _consumberHandler.Setup(x => x.Topic).Returns("topic1");
            _consumberHandler.Setup(x => x.GroupId).Returns("GroupId1");
            _consumberHandler.Raise(x => x.ProcessCompleted += Consumer_ProcessCompleted);
                
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

            DataProcessService dataProcess = new DataProcessService(_loggerMock.Object, _consumberHandler.Object, _producerHandler.Object);
            dataProcess.ProcessData();
            _consumberHandler.Verify(x => x.Subscribe(), Times.Once);
        }

        private void Producer_ProcessCompleted(string data)
        {
            Console.WriteLine(data);
        }

        private void Consumer_ProcessCompleted(string data)
        {
            Console.WriteLine(data);
        }
    }
}
