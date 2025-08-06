using IntermissionsPwaApp.Entities;

namespace IntermissionsPwaApp.Services
{
    public interface IFutureReleaseService
    {
        Task<List<FutureRelease>> GetAllAsync();
        Task AddAsync(FutureRelease release);
        Task DeleteAsync(string id);
        Task<FutureRelease?> GetByIdAsync(string id);
        Task UpdateAsync(FutureRelease release);
        Task MoveToShowAsync(string id);
    }
}