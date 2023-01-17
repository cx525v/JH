using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SampleService.Interfaces;
using SampleService.Services;
using SharedLibrary.Models;
using Xunit;

namespace UnitTesting
{
    public class SampleServiceTweetClientUnitTests
    {
        private Mock<ILogger<TweetClient>> loggerMock;
        private readonly IConfiguration configuration;
        private readonly Mock<IAppHttpClientHandler> _httpClient;
        private readonly Mock<IPublishService> _publishService;
        public SampleServiceTweetClientUnitTests()
        {
            loggerMock = new Mock<ILogger<TweetClient>>();
            _httpClient = new Mock<IAppHttpClientHandler>();
            _publishService = new Mock<IPublishService>();
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) 
                .AddJsonFile(@"appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();
        }
        [Fact]
        public async Task GetTweetDataTest_successful()
        {
            _publishService.Setup(x => x.Publish(It.IsAny<TwitterRecord>(), CancellationToken.None));

            byte[] file = await File.ReadAllBytesAsync(Directory.GetCurrentDirectory() + "\\data.txt");

            Stream stream = new MemoryStream(file);

            _httpClient.Setup(x => x.GetStreamAsync()).Returns(Task.FromResult(stream)).Verifiable();

            TweetClient client = new TweetClient(loggerMock.Object, _httpClient.Object, _publishService.Object);

            await client.GetTweetData(CancellationToken.None);

            _httpClient.Verify(x =>x.GetStreamAsync(), Times.Once);
        }


        [Fact]
        public async Task GetTweetDataTest_cancel()
        {
           
            byte[] file = await File.ReadAllBytesAsync(Directory.GetCurrentDirectory() + "\\data.txt");

            Stream stream = new MemoryStream(file);

            _httpClient.Setup(x => x.GetStreamAsync()).Returns(Task.FromResult(stream)).Verifiable();

            TweetClient client = new TweetClient(loggerMock.Object, _httpClient.Object, _publishService.Object);


            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            source.Cancel();
            _publishService.Setup(x => x.Publish(It.IsAny<TwitterRecord>(), token));

            await client.GetTweetData(token);

            _httpClient.Verify(x => x.GetStreamAsync(), Times.Once);
        }
    }
}