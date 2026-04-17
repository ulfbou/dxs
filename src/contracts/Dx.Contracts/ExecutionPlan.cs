using System.Security.Cryptography;
using System.Text;

namespace Dx.Contracts;

public enum ExecutionMode { Apply, DryRun, Request }
public enum BlockType { File, Patch, Fs, Request, Checkout }

public sealed record PlannedBlock(BlockType BlockType, string BlockHash, string Payload);

public sealed record ExecutionPlan(string PlanId, IReadOnlyList<PlannedBlock> PlannedBlocks, ExecutionMode Mode, bool IsDryRun)
{
    public string PlanHash => ComputeHash(PlannedBlocks);

    public static string ComputeHash(IReadOnlyList<PlannedBlock> blocks)
    {
        var canonical = string.Join("\n", blocks.Select(b => $"{b.BlockType}|{b.BlockHash}|{b.Payload}"));
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical)));
    }
}

public interface IExecutionContext
{
    bool IsDryRun { get; }
    CancellationToken CancellationToken { get; }
    Task StageFileWriteAsync(string path, string content);
}

public interface IBlockExecutor
{
    BlockType BlockType { get; }
    Task ExecuteAsync(PlannedBlock block, IExecutionContext context);
}

public sealed record DxResult(string ExecutionId, string SnapshotId, bool SnapshotExpected, bool Success, string? Error = null);
