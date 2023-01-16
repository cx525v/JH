using SampleService.Interfaces;

namespace SampleService.Services
{
    public class AppHttpClientHandler: IAppHttpClientHandler
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly IConfiguration _configuration;

        public AppHttpClientHandler(IConfiguration configuration )
        {
            _configuration = configuration;        
        }
        public async Task<Stream> GetStreamAsync()
        {
            string baseUrl = _configuration.GetValue("TweetApi:SampleStreamBaseUrl", "");
            string token = _configuration.GetValue("TweetApi:BearerToken", "");

            _client.BaseAddress = new Uri($"{baseUrl}?tweet.fields=entities");
            _client.DefaultRequestHeaders.Add("User-Agent", "JHA");
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            return await _client.GetStreamAsync("");
        }
    }
}
