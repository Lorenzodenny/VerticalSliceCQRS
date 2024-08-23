namespace WebAppApi.Database.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        eCommerceDbContext Context { get; }
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
