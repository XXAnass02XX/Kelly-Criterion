using KellyCriterion.Core.Domain;

namespace KellyCriterion.Core.Simulation;

public sealed record SimulationResult(
    SimulationParameters Parameters,
    string StrategyName,
    double WagerFraction,
    IReadOnlyList<double[]> WealthPaths,
    double[] AverageWealthPath,
    double[] MedianWealthPath)
{
    public SimulationSummary Summary { get; } = SimulationSummary.From(Parameters, WealthPaths, AverageWealthPath, MedianWealthPath);

    public double ExpectedLogGrowthPerGamble => KellyFormula.ExpectedLogGrowth(Parameters.Odds, WagerFraction);
}
