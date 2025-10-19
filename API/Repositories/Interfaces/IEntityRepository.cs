using API.Models;

namespace API.Repositories.Interfaces;

public interface IEntityRepository : IGenericRepository<Entity>
{
    Task<Entity?> GetByCodeAsync(string code);
    Task<IEnumerable<Entity>> GetByCategoryAsync(string category);
    Task<IEnumerable<Entity>> GetByStatusAsync(string status);
    Task<IEnumerable<Entity>> GetActiveEntitiesAsync();
    Task<IEnumerable<Entity>> GetFeaturedEntitiesAsync();
    Task<IEnumerable<Entity>> GetByPriorityRangeAsync(int minPriority, int maxPriority);
    Task<bool> IsCodeExistsAsync(string code);
    Task<IEnumerable<Entity>> SearchByNameAsync(string searchTerm);
}