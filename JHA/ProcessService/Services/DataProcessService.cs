using Confluent.Kafka;
using Newtonsoft.Json;
using ProcessService.Interfaces;
using SharedLibrary.Models;

namespace ProcessService.Services
{
    public class DataProcessService: IDataProcessService
    {
       // static readonly object tweet = new object();
        readonly TweetResponse tweetResponse = new TweetResponse();

        //get data from producer with the topic
        public void ProcessData()
        {
            var config = new ConsumerConfig
            {
                GroupId = "test-consumer-group",
                BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS"),
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe("tweetRawDataTopic");
                try
                {
                    while (true)
                    {
                        try
                        {
                            var cr = consumer.Consume();

                            if (cr.Message.Value != null)
                            {
                                List<TwitterRecord> BulkRecords = JsonConvert.DeserializeObject<List<TwitterRecord>>(cr.Message.Value);

                                UpdateData(BulkRecords);
                            }

                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Error occured: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    consumer.Close();
                }
            }
        }

       //data can be saved into database, here upsert into memory
        private void UpdateData(List<TwitterRecord> records)
        {

            IDictionary<string, int> hashDict = new Dictionary<string, int>();

            records.ForEach((record) =>
            {
                record.Data.Entities.Hashtags.ForEach(tag =>
                {
                    if (tag != null)
                    {
                        if (!string.IsNullOrEmpty(tag.Tag) && hashDict.ContainsKey(tag.Tag))
                        {
                            hashDict[tag.Tag] = int.Parse(hashDict[tag.Tag].ToString()) + 1;
                        }
                        else
                        {
                            hashDict[tag.Tag] = 1;
                        }
                    }

                });
            });
          
            tweetResponse.TweentTotalCount += records.Count;
            foreach (var dict in hashDict)
            {
                var key = dict.Key;
                if (tweetResponse.HashTags.ContainsKey(key))
                {
                    tweetResponse.HashTags[key] = tweetResponse.HashTags[key] + dict.Value;
                }
                else
                {
                    tweetResponse.HashTags[key] = dict.Value;
                }

            }

            tweetResponse.HashTags = tweetResponse.HashTags.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            
        }


        //Display info to console
        public void DisplayData()
        {
            if (tweetResponse.TweentTotalCount > 0)
            {
                Console.WriteLine("############################");
                Console.WriteLine($"{DateTimeOffset.Now}");

                Console.WriteLine($"Total Tweets Retrieved: {tweetResponse.TweentTotalCount}");
                Console.WriteLine("");
                Console.WriteLine($"Top 10 hash tags:");
                var top10Dict = (from entry in tweetResponse.HashTags
                                 orderby entry.Value descending
                                 select entry
                      ).Take(10)
                      .ToDictionary(pair => pair.Key, pair => pair.Value);


                foreach (var hash in top10Dict)
                {
                    Console.WriteLine($"\tHashTag: {hash.Key}    {hash.Value}");
                }

                Console.WriteLine("############################");
            }


        }

    }
}
