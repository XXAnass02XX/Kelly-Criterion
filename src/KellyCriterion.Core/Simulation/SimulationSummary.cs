using KellyCriterion.Core.Domain;

namespace KellyCriterion.Core.Simulation;

public sealed record SimulationSummary(
    double AvgFinalWealth,
    double MedianFinalWealth,
    double MinFinalWealth,
    double MaxFinalWealth,
    double ProfitableRunsRatio)
{
    public static SimulationSummary From(SimulationParameters parameters, IReadOnlyList<double[]> paths, double[] Avgwealth, double[] Medianwealth)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(Avgwealth);
        ArgumentNullException.ThrowIfNull(Medianwealth);

        var finals = paths.Select(p => p[^1]).Order().ToArray();
        var middle = finals.Length / 2;
        var median = finals.Length % 2 == 0 ? (finals[middle - 1] + finals[middle]) / 2 : finals[middle];
        var profitable = finals.Count(w => w > parameters.InitialWealth) / (double)finals.Length;

        return new SimulationSummary(Avgwealth[^1],  median, finals[0], finals[^1], profitable);
    }
}
