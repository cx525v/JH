// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using SharedLibrary.Constants;
using SharedLibrary.Handlers;
using SharedLibrary.Models;

Console.WriteLine("Getting Data");
ConsumerBuilderHandler consumer = new ConsumerBuilderHandler();
consumer.Topic = AppConstants.PROCESS_TOPIC;
consumer.GroupId = AppConstants.PROCESS_GROUP_ID;
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
        
            if (tweet.HashTags != null)
            {
                foreach (var hash in tweet.HashTags)
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
