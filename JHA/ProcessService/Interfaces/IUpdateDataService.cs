using SharedLibrary.Models;

namespace ProcessService.Interfaces
{
    public interface IUpdateDataService
    {
       Task UpdateData(List<TwitterRecord> records);
    }
}
