namespace SampleService.Interfaces
{
    public interface IAppHttpClientHandler
    {
        Task<Stream> GetStreamAsync();
    }
}
