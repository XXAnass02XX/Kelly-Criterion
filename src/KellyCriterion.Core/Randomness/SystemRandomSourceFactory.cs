namespace KellyCriterion.Core.Randomness;

public sealed class SystemRandomSourceFactory(int? seed = null) : IRandomSourceFactory
{
    public IRandomSource Create(int runIndex) =>
        new SystemRandomSource(seed is { } s ? new Random(HashCode.Combine(s, runIndex)) : new Random());

    private sealed class SystemRandomSource(Random random) : IRandomSource
    {
        public double NextDouble() => random.NextDouble();
    }
}
