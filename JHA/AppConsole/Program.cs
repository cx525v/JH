// See https://aka.ms/new-console-template for more information
using Confluent.Kafka;
using Newtonsoft.Json;
using SharedLibrary.Constants;
using SharedLibrary.Models;

Console.WriteLine("Getting Data");
var config = new ConsumerConfig
{
    GroupId = "test-data-group",
    BootstrapServers = "localhost:9092",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
{
    consumer.Subscribe(AppConstants.PROCESS_TOPIC);
    try
    {
        while (true)
        {
            try
            {
                var cr = consumer.Consume();

                if (cr!=null && cr.Message != null && cr.Message.Value != null)
                {
                   TweetResponse tweet = JsonConvert.DeserializeObject<TweetResponse>(cr.Message.Value);
                    if(tweet != null)
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

                        if(hashtags != null)
                        {
                            foreach (var hash in hashtags)
                            {
                                Console.WriteLine($"{hash.Key}   {hash.Value}");
                            }
                        }
                       
                        Console.WriteLine("----------------------------");
                    }
                  
                }
            }
            catch (ConsumeException e)
            {
                Console.WriteLine($"Error occured: {e.Error.Reason}");
            }
        }
    }
    catch (OperationCanceledException er)
    {
        Console.WriteLine(er.Message);
        consumer.Close();
    }
}
