using KellyCriterion.Core.Domain;
using KellyCriterion.Core.Randomness;
using KellyCriterion.Core.Simulation;
using KellyCriterion.Core.Strategies;

namespace KellyCriterion.Core.Tests;

public class GamblerSimulatorTests
{
    private sealed class ConstantRandomFactory(double value) : IRandomSourceFactory
    {
        public IRandomSource Create(int runIndex) => new Constant(value);

        private sealed class Constant(double value) : IRandomSource
        {
            public double NextDouble() => value;
        }
    }

    private sealed class PerRunRandomFactory(Func<int, double> valueForRun) : IRandomSourceFactory
    {
        public IRandomSource Create(int runIndex) => new Constant(valueForRun(runIndex));

        private sealed class Constant(double value) : IRandomSource
        {
            public double NextDouble() => value;
        }
    }

    private sealed class SequencePerRunRandomFactory(Func<int, double[]> sequenceForRun) : IRandomSourceFactory
    {
        public IRandomSource Create(int runIndex) => new Sequence(sequenceForRun(runIndex));

        private sealed class Sequence(double[] values) : IRandomSource
        {
            private int _next;

            public double NextDouble() => values[_next++];
        }
    }

    private sealed class FixedStrategy(double fraction) : IBettingStrategy
    {
        public string Name => "Fixed";

        public double FractionToWager(GambleOdds odds) => fraction;
    }

    [Fact]
    public void Simulate_ProducesOnePathPerRun_WithInitialPointIncluded()
    {
        var simulator = new GamblerSimulator(new SystemRandomSourceFactory(42));
        var parameters = new SimulationParameters(7, 25, new GambleOdds(0.6, 1), new KellyBettingStrategy());

        var result = simulator.Simulate(parameters);

        Assert.Equal(7, result.WealthPaths.Count);
        Assert.All(result.WealthPaths, p => Assert.Equal(26, p.Length));
        Assert.All(result.WealthPaths, p => Assert.Equal(1.0, p[0]));
        Assert.Equal(26, result.AverageWealthPath.Length);
        Assert.Equal(26, result.MedianWealthPath.Length);
    }

    [Fact]
    public void Simulate_WhenAlwaysWinning_CompoundsByOnePlusFractionTimesPayout()
    {
        var simulator = new GamblerSimulator(new ConstantRandomFactory(0.0));
        var odds = new GambleOdds(0.6, 2);
        var fraction = KellyFormula.OptimalFraction(odds);

        var result = simulator.Simulate(new SimulationParameters(3, 10, odds, new KellyBettingStrategy()));

        var expected = Math.Pow(1 + fraction * 2, 10);
        Assert.All(result.WealthPaths, p => Assert.Equal(expected, p[^1], precision: 10));
    }

    [Fact]
    public void Simulate_WhenAlwaysLosing_ShrinksByOneMinusFraction()
    {
        var simulator = new GamblerSimulator(new ConstantRandomFactory(0.999));

        var result = simulator.Simulate(new SimulationParameters(2, 5, new GambleOdds(0.6, 1), new FixedStrategy(0.1)));

        Assert.All(result.WealthPaths, p => Assert.Equal(Math.Pow(0.9, 5), p[^1], precision: 10));
    }

    [Fact]
    public void Simulate_AveragePath_IsPointwiseMeanOfRuns()
    {
        var simulator = new GamblerSimulator(new PerRunRandomFactory(run => run == 0 ? 0.0 : 0.999));

        var result = simulator.Simulate(new SimulationParameters(2, 1, new GambleOdds(0.5, 1), new FixedStrategy(0.5)));

        Assert.Equal((1.5 + 0.5) / 2, result.AverageWealthPath[1], precision: 10);
        Assert.Equal(0.5, result.Summary.ProfitableRunsRatio);
    }

    [Fact]
    public void Simulate_MedianPath_OddRunCount_IsMiddleValueOfRuns()
    {
        var simulator = new GamblerSimulator(new PerRunRandomFactory(run => run == 1 ? 0.999 : 0.0));

        var result = simulator.Simulate(new SimulationParameters(3, 1, new GambleOdds(0.5, 1), new FixedStrategy(0.5)));

        Assert.Equal(1.0, result.MedianWealthPath[0], precision: 10);
        Assert.Equal(1.5, result.MedianWealthPath[1], precision: 10);
    }

    [Fact]
    public void Simulate_MedianPath_EvenRunCount_IsMeanOfTwoMiddleValues()
    {
        var sequences = new[]
        {
            new[] { 0.0, 0.0 },
            new[] { 0.0, 0.999 },
            new[] { 0.999, 0.999 },
            new[] { 0.0, 0.0 },
        };
        var simulator = new GamblerSimulator(new SequencePerRunRandomFactory(run => sequences[run]));

        var result = simulator.Simulate(new SimulationParameters(4, 2, new GambleOdds(0.5, 1), new FixedStrategy(0.5)));

        Assert.Equal((0.75 + 2.25) / 2, result.MedianWealthPath[2], precision: 10);
    }

    [Fact]
    public void Simulate_MedianPath_SingleRun_IsThatRun()
    {
        var simulator = new GamblerSimulator(new SystemRandomSourceFactory(3));

        var result = simulator.Simulate(new SimulationParameters(1, 20, new GambleOdds(0.6, 1), new KellyBettingStrategy()));

        Assert.Equal(result.WealthPaths[0], result.MedianWealthPath);
    }

    [Fact]
    public void Simulate_WithSameSeed_IsReproducible()
    {
        var parameters = new SimulationParameters(5, 100, new GambleOdds(0.55, 1), new KellyBettingStrategy());

        var first = new GamblerSimulator(new SystemRandomSourceFactory(7)).Simulate(parameters);
        var second = new GamblerSimulator(new SystemRandomSourceFactory(7)).Simulate(parameters);

        Assert.Equal(first.AverageWealthPath, second.AverageWealthPath);
        Assert.Equal(first.MedianWealthPath, second.MedianWealthPath);
    }

    [Fact]
    public void Simulate_WhenCancelled_Throws()
    {
        var simulator = new GamblerSimulator(new SystemRandomSourceFactory());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() =>
            simulator.Simulate(new SimulationParameters(10, 10, new GambleOdds(0.6, 1), new KellyBettingStrategy()), cancellationToken: cts.Token));
    }

    [Fact]
    public void Simulate_UsesFractionFromStrategyInParameters()
    {
        var simulator = new GamblerSimulator(new ConstantRandomFactory(0.0));
        var odds = new GambleOdds(0.6, 1);

        var full = simulator.Simulate(new SimulationParameters(1, 1, odds, new KellyBettingStrategy()));
        var half = simulator.Simulate(new SimulationParameters(1, 1, odds, new HalfKellyStrategy()));

        Assert.Equal(0.2, full.WagerFraction, precision: 10);
        Assert.Equal(0.1, half.WagerFraction, precision: 10);
    }

    // Progress<T> would post callbacks to the thread pool in a test, so assertions could run before they arrive.
    private sealed class RecordingProgress : IProgress<int>
    {
        private readonly Lock _gate = new();
        private readonly List<int> _values = [];

        public IReadOnlyList<int> Values
        {
            get { lock (_gate) return [.. _values]; }
        }

        public void Report(int value)
        {
            lock (_gate) _values.Add(value);
        }
    }

    [Fact]
    public void Simulate_ReportsStrictlyIncreasingPercentagesEndingAt100()
    {
        var simulator = new GamblerSimulator(new SystemRandomSourceFactory(1));
        var progress = new RecordingProgress();

        simulator.Simulate(new SimulationParameters(300, 500, new GambleOdds(0.6, 1), new KellyBettingStrategy()), progress);

        var values = progress.Values;
        Assert.NotEmpty(values);
        Assert.Equal(100, values[^1]);
        Assert.All(values, v => Assert.InRange(v, 0, 100));
        for (var i = 1; i < values.Count; i++)
            Assert.True(values[i] > values[i - 1], $"Progress went from {values[i - 1]} to {values[i]}.");
    }
}
