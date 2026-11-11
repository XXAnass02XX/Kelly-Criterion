using KellyCriterion.Core.Domain;
using KellyCriterion.Core.Randomness;
using KellyCriterion.Core.Strategies;

namespace KellyCriterion.Core.Simulation;

public sealed class GamblerSimulator(IRandomSourceFactory randomSourceFactory) : IGamblerSimulator
{
    private const int CancellationCheckMask = 0xFFF;

    // Measured share of total time: the sort-based median dominates, the average is a single cheap pass.
    private const int RunsEndPercent = 20;
    private const int AverageEndPercent = 25;

    public SimulationResult Simulate(SimulationParameters parameters, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var reporter = new PercentReporter(progress);
        var fraction = parameters.Strategy.FractionToWager(parameters.Odds);
        var paths = new double[parameters.Runs][];
        var options = new ParallelOptions { CancellationToken = cancellationToken };
        var completed = 0;

        Parallel.For(0, parameters.Runs, options, run =>
        {
            paths[run] = SimulateRun(parameters, fraction, randomSourceFactory.Create(run), cancellationToken);
            var done = Interlocked.Increment(ref completed);
            reporter.Report(0, RunsEndPercent, done, parameters.Runs);
        });

        var average = ComputeAverage(paths, reporter, cancellationToken);
        var median = ComputeMedian(paths, reporter, cancellationToken);
        return new SimulationResult(parameters, parameters.Strategy.Name, fraction, paths, average, median);
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

    private static double[] ComputeAverage(double[][] paths, PercentReporter reporter, CancellationToken cancellationToken)
    {
        var average = new double[paths[0].Length];
        for (var run = 0; run < paths.Length; run++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = paths[run];
            for (var i = 0; i < average.Length; i++)
                average[i] += path[i];

            reporter.Report(RunsEndPercent, AverageEndPercent, run + 1, paths.Length);
        }

        for (var i = 0; i < average.Length; i++)
            average[i] /= paths.Length;

        return average;
    }

    private static double[] ComputeMedian(double[][] paths, PercentReporter reporter, CancellationToken cancellationToken)
    {
        var median = new double[paths[0].Length];
        var valuesAtStep = new double[paths.Length];
        for (var i = 0; i < median.Length; i++)
        {
            if ((i & CancellationCheckMask) == 0)
                cancellationToken.ThrowIfCancellationRequested();

            for (var run = 0; run < paths.Length; run++)
                valuesAtStep[run] = paths[run][i];

            median[i] = SortAndTakeMedian(valuesAtStep);
            reporter.Report(AverageEndPercent, 100, i + 1, median.Length);
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

    private sealed class PercentReporter(IProgress<int>? progress)
    {
        private readonly Lock _gate = new();
        private int _lastPercent = -1;

        public void Report(int phaseStartPercent, int phaseEndPercent, long done, long total)
        {
            if (progress is null)
                return;

            var percent = phaseStartPercent + (int)((phaseEndPercent - phaseStartPercent) * done / total);

            // Parallel runs can finish their Interlocked increment in one order and reach this line in another;
            // the lock plus the "only go up" check keeps the reported sequence strictly increasing.
            lock (_gate)
            {
                if (percent <= _lastPercent)
                    return;

                _lastPercent = percent;
                progress.Report(percent);
            }
        }
    }
}
