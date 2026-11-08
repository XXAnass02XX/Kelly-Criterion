namespace KellyCriterion.Core.Randomness;

public interface IRandomSource
{
    double NextDouble();
}

public interface IRandomSourceFactory
{
    IRandomSource Create(int runIndex);
}
