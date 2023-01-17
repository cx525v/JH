using Confluent.Kafka;
using SharedLibrary.Handlers.Interfaces;

namespace SharedLibrary.Handlers
{
    public class ProducerBuilderHandler : IProducerBuilderHandler
    {
        public string BootstrapServers { get; set; }
        public string Topic { get; set; }
        public string Data { get; set; }

        public event Notify ProcessCompleted;
        public ProducerBuilderHandler()
        {

        }

        protected virtual void OnProcessCompleted(string data)
        {
            ProcessCompleted?.Invoke(data);
        }

        public async Task ProduceAsync()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = BootstrapServers
            };

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    await producer.ProduceAsync(Topic, new Message<Null, string> { Value = Data });
                    OnProcessCompleted(Data);
                }
                catch (ProduceException<Null, string> e)
                {
                    Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                }
            }
        }
    }
}
