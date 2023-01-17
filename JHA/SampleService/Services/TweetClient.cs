using Newtonsoft.Json;
using SampleService.Interfaces;
using SharedLibrary.Models;
using System.Text;

namespace SampleService.Services
{
    public class TweetClient : ITweetClient
    {
        private readonly ILogger<TweetClient> _logger;
        private readonly IAppHttpClientHandler _httpClient;
        private readonly IPublishService _publishService;
        public TweetClient(ILogger<TweetClient> logger, IAppHttpClientHandler httpClient, IPublishService publishService)
        {
            _logger = logger;
            _httpClient = httpClient;      
            _publishService = publishService;
        }


        //get twitter sample stream
        public async Task GetTweetData(CancellationToken cancellationToken)
        {      
            using var response = await _httpClient.GetStreamAsync();
            using var sr = new StreamReader(response, Encoding.UTF8, true, 16384, leaveOpen: true);       
            while (!cancellationToken.IsCancellationRequested && !sr.EndOfStream)
            {
                try
                {
                    var res = await sr.ReadLineAsync().ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(res))
                    {
                        var record = JsonConvert.DeserializeObject<TwitterRecord>(res);
                        if (record != null)
                        {
                            await _publishService.Publish(record, cancellationToken);
                        }
                    }

                    if (cancellationToken.IsCancellationRequested) break;
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
               
            }

        }
  
    }
}
