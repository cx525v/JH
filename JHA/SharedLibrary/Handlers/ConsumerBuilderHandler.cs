using Confluent.Kafka;
using SharedLibrary.Handlers.Interfaces;

namespace SharedLibrary.Handlers
{
    public delegate void Notify(string data);
    public class ConsumerBuilderHandler: IConsumerBuilderHandler
    {
        public string GroupId { get; set; }
        public string BootstrapServers { get; set; }
        public string Topic { get; set; }

        public event Notify ProcessCompleted;
        public ConsumerBuilderHandler()
        {

        }

        protected virtual void OnProcessCompleted(string data) 
        {
            ProcessCompleted?.Invoke(data);
        }

        public void Subscribe()
        {
            var config = new ConsumerConfig
            {
                GroupId = GroupId,
                BootstrapServers = BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe(Topic);
                try
                {
                    while (true)
                    {
                        try
                        {
                            var cr = consumer.Consume();

                            if (!string.IsNullOrEmpty(cr.Message.Value))
                            {
                               OnProcessCompleted(cr.Message.Value);
                             
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
        }
    }
}
