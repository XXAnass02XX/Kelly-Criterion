using KellyCriterion.Core.Domain;

namespace KellyCriterion.Core.Simulation;

public sealed record SimulationResult(
    SimulationParameters Parameters,
    string StrategyName,
    double WagerFraction,
    IReadOnlyList<double[]> WealthPaths,
    double[] WealthPath)
{
    public SimulationSummary Summary { get; } = SimulationSummary.From(Parameters, WealthPaths, WealthPath);

    public double ExpectedLogGrowthPerGamble => KellyFormula.ExpectedLogGrowth(Parameters.Odds, WagerFraction);
}
