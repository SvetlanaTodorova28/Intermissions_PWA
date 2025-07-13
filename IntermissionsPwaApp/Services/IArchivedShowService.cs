// IArchivedShowService.cs
using IntermissionsPwaApp.Entities;

namespace IntermissionsPwaApp.Services;

public interface IArchivedShowService
{
    Task<List<Show>> GetAllAsync();
    Task AddAsync(Show newShow);
    Task DeleteAsync(string id);
    Task UnarchiveShowAsync(string id);
    Task ExportToCsvAsync();
    Task ImportFromCsvAsync(string csvContent);
}