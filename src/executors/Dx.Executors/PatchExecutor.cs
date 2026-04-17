using Dx.Contracts;

namespace Dx.Executors;

public sealed class PatchExecutor : IBlockExecutor
{
    public BlockType BlockType => BlockType.Patch;
    public Task ExecuteAsync(PlannedBlock block, IExecutionContext context)
        => context.StageFileWriteAsync("dummy.txt", block.Payload);
}
