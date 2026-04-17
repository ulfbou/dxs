using Dx.Contracts;
using Dx.Core.Engine.Abstractions;
using Dx.Infrastructure.Abstractions;

namespace Dx.Core.Engine;

public sealed class Engine : IEngine
{
    private readonly IDxStore _store;
    private readonly IReadOnlyDictionary<BlockType, IBlockExecutor> _executors;

    public Engine(IDxStore store, IEnumerable<IBlockExecutor> executors)
    {
        _store = store;
        _executors = executors.ToDictionary(e => e.BlockType);
    }

    public async Task<DxResult> ExecutePlanAsync(ExecutionPlan plan, CancellationToken ct)
    {
        var snapshotId = Guid.NewGuid().ToString("N");
        await _store.BeginTransactionAsync(ct);
        await _store.CommitAsync(ct);
        return new DxResult(Guid.NewGuid().ToString(), snapshotId, true, true);
    }
}
