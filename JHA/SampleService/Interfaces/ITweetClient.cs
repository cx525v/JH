namespace SampleService.Interfaces
{
    public interface ITweetClient
    {
        Task GetTweetData(CancellationToken cancellationToken);
    }
}
