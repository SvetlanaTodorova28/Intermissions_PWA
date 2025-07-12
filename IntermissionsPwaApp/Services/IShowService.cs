using IntermissionsPwaApp.Entities;

namespace IntermissionsPwaApp.Services;

public interface IShowService
{
    Task<List<Show>> GetAllAsync();
    Task<Show?> GetByIdAsync(string id);
    Task CreateAsync(Show newShow);
    Task UpdateAsync(Show updatedShow);
    Task DeleteAsync(string id);
    Task UpdateBadgeAsync();
}