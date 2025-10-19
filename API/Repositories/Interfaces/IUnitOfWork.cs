using API.Repositories.Interfaces;

namespace API.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IEntityRepository Entities { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}