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

    private sealed class FixedStrategy(double fraction) : IBettingStrategy
    {
        public string Name => "Fixed";

        public double FractionToWager(GambleOdds odds) => fraction;
    }

    [Fact]
    public void Simulate_ProducesOnePathPerRun_WithInitialPointIncluded()
    {
        var simulator = new GamblerSimulator(new KellyBettingStrategy(), new SystemRandomSourceFactory(42));
        var parameters = new SimulationParameters(7, 25, new GambleOdds(0.6, 1));

        var result = simulator.Simulate(parameters);

        Assert.Equal(7, result.WealthPaths.Count);
        Assert.All(result.WealthPaths, p => Assert.Equal(26, p.Length));
        Assert.All(result.WealthPaths, p => Assert.Equal(1.0, p[0]));
        Assert.Equal(26, result.AverageWealthPath.Length);
    }

    [Fact]
    public void Simulate_WhenAlwaysWinning_CompoundsByOnePlusFractionTimesPayout()
    {
        var simulator = new GamblerSimulator(new KellyBettingStrategy(), new ConstantRandomFactory(0.0));
        var odds = new GambleOdds(0.6, 2);
        var fraction = KellyFormula.OptimalFraction(odds);

        var result = simulator.Simulate(new SimulationParameters(3, 10, odds));

        var expected = Math.Pow(1 + fraction * 2, 10);
        Assert.All(result.WealthPaths, p => Assert.Equal(expected, p[^1], precision: 10));
    }

    [Fact]
    public void Simulate_WhenAlwaysLosing_ShrinksByOneMinusFraction()
    {
        var simulator = new GamblerSimulator(new FixedStrategy(0.1), new ConstantRandomFactory(0.999));

        var result = simulator.Simulate(new SimulationParameters(2, 5, new GambleOdds(0.6, 1)));

        Assert.All(result.WealthPaths, p => Assert.Equal(Math.Pow(0.9, 5), p[^1], precision: 10));
    }

    [Fact]
    public void Simulate_AveragePath_IsPointwiseMeanOfRuns()
    {
        var simulator = new GamblerSimulator(new FixedStrategy(0.5), new PerRunRandomFactory(run => run == 0 ? 0.0 : 0.999));

        var result = simulator.Simulate(new SimulationParameters(2, 1, new GambleOdds(0.5, 1)));

        Assert.Equal((1.5 + 0.5) / 2, result.AverageWealthPath[1], precision: 10);
        Assert.Equal(0.5, result.Summary.ProfitableRunsRatio);
    }

    [Fact]
    public void Simulate_WithSameSeed_IsReproducible()
    {
        var parameters = new SimulationParameters(5, 100, new GambleOdds(0.55, 1));

        var first = new GamblerSimulator(new KellyBettingStrategy(), new SystemRandomSourceFactory(7)).Simulate(parameters);
        var second = new GamblerSimulator(new KellyBettingStrategy(), new SystemRandomSourceFactory(7)).Simulate(parameters);

        Assert.Equal(first.AverageWealthPath, second.AverageWealthPath);
    }

    [Fact]
    public void Simulate_WhenCancelled_Throws()
    {
        var simulator = new GamblerSimulator(new KellyBettingStrategy(), new SystemRandomSourceFactory());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() =>
            simulator.Simulate(new SimulationParameters(10, 10, new GambleOdds(0.6, 1)), cts.Token));
    }
}
