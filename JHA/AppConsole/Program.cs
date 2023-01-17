// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using SharedLibrary.Constants;
using SharedLibrary.Handlers;
using SharedLibrary.Models;

Console.WriteLine("Getting Data");
ConsumerBuilderHandler consumer = new ConsumerBuilderHandler();
consumer.Topic = AppConstants.PROCESS_TOPIC;
consumer.GroupId = "test-data-group";
consumer.BootstrapServers = "localhost:9092";
consumer.ProcessCompleted += Consumer_ProcessCompleted;
consumer.Subscribe();

void Consumer_ProcessCompleted(string data)
{
    if(string.IsNullOrEmpty(data))
    {
        return;
    }
    try
    {
        TweetResponse? tweet = JsonConvert.DeserializeObject<TweetResponse>(data);
        if (tweet != null)
        {
            Console.WriteLine("----------------------------");
            Console.WriteLine($"Total Tweets: {tweet.TweentTotalCount}");
            Console.WriteLine();
            Console.WriteLine("Top 10 hashtags:");
            var hashtags = (from entry in tweet.HashTags
                            orderby entry.Value descending
                            select entry
                      ).Take(10)
                      .ToDictionary(pair => pair.Key, pair => pair.Value);

            if (hashtags != null)
            {
                foreach (var hash in hashtags)
                {
                    Console.WriteLine($"{hash.Key}   {hash.Value}");
                }
            }

            Console.WriteLine("----------------------------");
        }
    }
    catch(Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
   

}
