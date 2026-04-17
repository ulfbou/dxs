using Dx.Contracts;

namespace Dx.Executors;

public sealed class CheckoutExecutor : IBlockExecutor
{
    public BlockType BlockType => BlockType.Checkout;
    public Task ExecuteAsync(PlannedBlock block, IExecutionContext context)
        => context.StageFileWriteAsync("dummy.txt", block.Payload);
}
