using Dx.Contracts;

namespace Dx.Core.Planning;

public static class PlanCompletenessHarness
{
    public static void ValidateAtBuild(IEnumerable<Type> executorTypes)
    {
        var types = executorTypes.Where(t => typeof(IBlockExecutor).IsAssignableFrom(t) && !t.IsInterface).ToList();
        var instances = types.Select(t => (IBlockExecutor)Activator.CreateInstance(t)!).ToList();

        foreach (BlockType bt in Enum.GetValues(typeof(BlockType)))
        {
            if (!instances.Any(i => i.BlockType == bt))
                throw new Exception($"Missing executor for {bt}");
        }
    }
}
