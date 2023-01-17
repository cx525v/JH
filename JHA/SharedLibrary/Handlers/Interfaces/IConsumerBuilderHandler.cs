namespace SharedLibrary.Handlers.Interfaces
{
    public interface IConsumerBuilderHandler
    {
        string GroupId { get; set; }
        string BootstrapServers { get; set; }
        string Topic { get; set; }

        event Notify ProcessCompleted;

        void Subscribe();
    }
}
