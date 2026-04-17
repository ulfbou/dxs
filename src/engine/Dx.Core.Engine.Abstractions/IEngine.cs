using Dx.Contracts;

namespace Dx.Core.Engine.Abstractions;

public interface IEngine
{
    Task<DxResult> ExecutePlanAsync(ExecutionPlan plan, CancellationToken ct);
}
