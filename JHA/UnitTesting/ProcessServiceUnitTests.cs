using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using ProcessService.Interfaces;
using ProcessService.Services;
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

        private readonly Mock<IUpdateDataService> _updateDataService;
        public ProcessServiceUnitTests()
        {
            configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile(@"appsettings.json", false, false)
              .AddEnvironmentVariables()
              .Build();

            _loggerMock = new Mock<ILogger<DataProcessService>>();
            _consumberHandler = new Mock<IConsumerBuilderHandler>();
       
            _updateDataService = new Mock<IUpdateDataService>();
        }

        [Fact]
        public void ProcessData_Successfully()
        {
            _consumberHandler.Setup(x => x.Subscribe()).Verifiable();
            _consumberHandler.Setup(x => x.BootstrapServers).Returns("BootstrapServers");
            _consumberHandler.Setup(x => x.Topic).Returns("topic1");
            _consumberHandler.Setup(x => x.GroupId).Returns("GroupId1");
            _consumberHandler.Raise(x => x.ProcessCompleted += Consumer_ProcessCompleted);

            _updateDataService.Setup(x => x.UpdateData(It.IsAny<List<TwitterRecord>>())).Verifiable();
           

            DataProcessService dataProcess = new DataProcessService(_loggerMock.Object, _consumberHandler.Object, _updateDataService.Object);
            dataProcess.ProcessData();
            _consumberHandler.Verify(x => x.Subscribe(), Times.Once);
  
        }

        private void Consumer_ProcessCompleted(string data)
        {
            Console.WriteLine(data);
        }

    }
}
