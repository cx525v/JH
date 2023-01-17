using SharedLibrary.Models;

namespace SampleService.Interfaces
{
    public interface IPublishService
    {
        Task Publish(TwitterRecord record, CancellationToken cancellationToken);
        List<TwitterRecord> BulkRecords { get; set; }
    }
}
