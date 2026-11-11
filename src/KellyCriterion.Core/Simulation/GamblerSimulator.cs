using KellyCriterion.Core.Domain;
using KellyCriterion.Core.Randomness;
using KellyCriterion.Core.Strategies;

namespace KellyCriterion.Core.Simulation;

public sealed class GamblerSimulator(IRandomSourceFactory randomSourceFactory) : IGamblerSimulator
{
    private const int CancellationCheckMask = 0xFFF;

    public SimulationResult Simulate(SimulationParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var fraction = parameters.Strategy.FractionToWager(parameters.Odds);
        var paths = new double[parameters.Runs][];
        var options = new ParallelOptions { CancellationToken = cancellationToken };

        Parallel.For(0, parameters.Runs, options, run =>
            paths[run] = SimulateRun(parameters, fraction, randomSourceFactory.Create(run), cancellationToken));

        return new SimulationResult(parameters, parameters.Strategy.Name, fraction, paths, ComputeAverage(paths), ComputeMedian(paths));
    }

    private static double[] SimulateRun(SimulationParameters parameters, double fraction, IRandomSource random, CancellationToken cancellationToken)
    {
        var odds = parameters.Odds;
        var path = new double[parameters.GamblesPerRun + 1];
        var wealth = parameters.InitialWealth;
        path[0] = wealth;

        for (var gamble = 1; gamble < path.Length; gamble++)
        {
            if ((gamble & CancellationCheckMask) == 0)
                cancellationToken.ThrowIfCancellationRequested();

            var stake = wealth * fraction;
            wealth += random.NextDouble() < odds.WinProbability ? stake * odds.PayoutRatio : -stake;
            path[gamble] = wealth;
        }

        return path;
    }

    private static double[] ComputeAverage(double[][] paths)
    {
        var average = new double[paths[0].Length];
        foreach (var path in paths)
        {
            for (var i = 0; i < average.Length; i++)
                average[i] += path[i];
        }

        for (var i = 0; i < average.Length; i++)
            average[i] /= paths.Length;

        return average;
    }

    private static double[] ComputeMedian(double[][] paths)
    {
        var median = new double[paths[0].Length];
        var valuesAtStep = new double[paths.Length];
        for (var i = 0; i < median.Length; i++)
        {
            for (var run = 0; run < paths.Length; run++)
                valuesAtStep[run] = paths[run][i];

            median[i] = SortAndTakeMedian(valuesAtStep);
        }

        return median;
    }

    private static double SortAndTakeMedian(double[] values)
    {
        Array.Sort(values);
        var mid = values.Length / 2;
        return values.Length % 2 == 1
            ? values[mid]
            : (values[mid - 1] + values[mid]) / 2;
    }
}
