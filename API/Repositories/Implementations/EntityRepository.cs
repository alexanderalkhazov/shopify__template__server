using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Models;
using API.Repositories.Interfaces;

namespace API.Repositories.Implementations;

public class EntityRepository : GenericRepository<Entity>, IEntityRepository
{
    public EntityRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Entity?> GetByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Code == code);
    }

    public async Task<IEnumerable<Entity>> GetByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(e => e.Category == category && e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entity>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Where(e => e.Status == status)
            .OrderBy(e => e.Priority)
            .ThenBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entity>> GetActiveEntitiesAsync()
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .OrderBy(e => e.Priority)
            .ThenBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entity>> GetFeaturedEntitiesAsync()
    {
        return await _dbSet
            .Where(e => e.IsFeatured && e.IsActive)
            .OrderBy(e => e.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entity>> GetByPriorityRangeAsync(int minPriority, int maxPriority)
    {
        return await _dbSet
            .Where(e => e.Priority >= minPriority && e.Priority <= maxPriority && e.IsActive)
            .OrderBy(e => e.Priority)
            .ToListAsync();
    }

    public async Task<bool> IsCodeExistsAsync(string code)
    {
        return await _dbSet.AnyAsync(e => e.Code == code);
    }

    public async Task<IEnumerable<Entity>> SearchByNameAsync(string searchTerm)
    {
        return await _dbSet
            .Where(e => e.Name.Contains(searchTerm) && e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Entity>> GetAllAsync()
    {
        return await _dbSet
            .OrderBy(e => e.Priority)
            .ThenBy(e => e.Name)
            .ToListAsync();
    }
}