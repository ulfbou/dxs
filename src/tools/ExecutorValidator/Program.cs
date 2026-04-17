using Dx.Contracts;

using System.Reflection;

var asm = typeof(Dx.Executors.FileExecutor).Assembly;

var implemented = asm.GetTypes()
    .Where(type => typeof(IBlockExecutor).IsAssignableFrom(type) && !type.IsAbstract)
    .Select(type => ((IBlockExecutor)Activator.CreateInstance(type)!).BlockType)
    .ToHashSet();

var missing = Enum.GetValues<BlockType>().Except(implemented).ToArray();

if (missing.Any())
{
    Console.Error.WriteLine($"VALIDATOR FAILED: Saknar executors för {string.Join(", ", missing)}");
    return 1;
}
Console.WriteLine("All executors validated.");
return 0;
