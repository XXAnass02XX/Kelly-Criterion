using KellyCriterion.Core.Domain;

namespace KellyCriterion.Core.Tests;

public class DomainValidationTests
{
    [Theory]
    [InlineData(-0.1, 1)]
    [InlineData(1.1, 1)]
    [InlineData(double.NaN, 1)]
    [InlineData(0.5, 0)]
    [InlineData(0.5, -2)]
    [InlineData(0.5, double.PositiveInfinity)]
    public void GambleOdds_RejectsInvalidValues(double p, double b)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GambleOdds(p, b));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    [InlineData(SimulationLimits.MaxRuns + 1, 10)]
    [InlineData(10, SimulationLimits.MaxGamblesPerRun + 1)]
    [InlineData(SimulationLimits.MaxRuns, SimulationLimits.MaxGamblesPerRun)]
    public void SimulationParameters_RejectsOutOfRangeCounts(int runs, int gambles)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationParameters(runs, gambles, new GambleOdds(0.6, 1)));
    }
}
