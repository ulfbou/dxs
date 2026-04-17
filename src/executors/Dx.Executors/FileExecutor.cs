using Dx.Contracts;

namespace Dx.Executors;

public sealed class FileExecutor : IBlockExecutor
{
    public BlockType BlockType => BlockType.File;
    public Task ExecuteAsync(PlannedBlock block, IExecutionContext context)
        => context.StageFileWriteAsync("dummy.txt", block.Payload);
}
