namespace SharedLibrary.Handlers.Interfaces
{
    public interface IProducerBuilderHandler
    {
        string BootstrapServers { get; set; }
        string Topic { get; set; }

        string Data { get; set; }

        event Notify ProcessCompleted;
        Task ProduceAsync();
    }
}
