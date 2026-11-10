using KellyCriterion.Core.Domain;

namespace KellyCriterion.Core.Simulation;

public sealed record SimulationResult(
    SimulationParameters Parameters,
    string StrategyName,
    double WagerFraction,
    IReadOnlyList<double[]> WealthPaths,
    double[] AvgWealthPath,
    double[] MedianWealthPath)
{
    public SimulationSummary Summary { get; } = SimulationSummary.From(Parameters, WealthPaths, AvgWealthPath, MedianWealthPath);

    public double ExpectedLogGrowthPerGamble => KellyFormula.ExpectedLogGrowth(Parameters.Odds, WagerFraction);
}
