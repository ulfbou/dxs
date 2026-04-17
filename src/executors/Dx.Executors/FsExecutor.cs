using Dx.Contracts;

namespace Dx.Executors;

public sealed class FsExecutor : IBlockExecutor
{
    public BlockType BlockType => BlockType.Fs;
    public Task ExecuteAsync(PlannedBlock block, IExecutionContext context)
        => context.StageFileWriteAsync("dummy.txt", block.Payload);
}
