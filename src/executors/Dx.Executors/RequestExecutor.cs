using Dx.Contracts;

namespace Dx.Executors;

public sealed class RequestExecutor : IBlockExecutor
{
    public BlockType BlockType => BlockType.Request;
    public Task ExecuteAsync(PlannedBlock block, IExecutionContext context)
        => context.StageFileWriteAsync("dummy.txt", block.Payload);
}
