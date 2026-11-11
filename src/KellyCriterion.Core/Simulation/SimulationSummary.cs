using KellyCriterion.Core.Domain;

namespace KellyCriterion.Core.Simulation;

public sealed record SimulationSummary(
    double AverageFinalWealth,
    double MedianFinalWealth,
    double MinFinalWealth,
    double MaxFinalWealth,
    double ProfitableRunsRatio)
{
    public static SimulationSummary From(SimulationParameters parameters, IReadOnlyList<double[]> paths, double[] average, double[] median)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(average);
        ArgumentNullException.ThrowIfNull(median);

        var finals = paths.Select(p => p[^1]).ToArray();
        var profitable = finals.Count(w => w > parameters.InitialWealth) / (double)finals.Length;

        return new SimulationSummary(average[^1], median[^1], finals.Min(), finals.Max(), profitable);
    }
}
