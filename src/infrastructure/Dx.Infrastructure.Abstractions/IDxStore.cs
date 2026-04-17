using Dx.Contracts;

namespace Dx.Infrastructure.Abstractions;

public interface IDxStore
{
    Task BeginTransactionAsync(CancellationToken ct);
    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync(CancellationToken ct);
}
