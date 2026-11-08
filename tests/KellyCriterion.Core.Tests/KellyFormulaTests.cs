using KellyCriterion.Core.Domain;

namespace KellyCriterion.Core.Tests;

public class KellyFormulaTests
{
    [Theory]
    [InlineData(0.6, 1.0, 0.2)]
    [InlineData(0.5, 2.0, 0.25)]
    [InlineData(0.25, 4.0, 0.0625)]
    [InlineData(1.0, 1.0, 1.0)]
    public void OptimalFraction_ReturnsKellyFraction(double p, double b, double expected)
    {
        var fraction = KellyFormula.OptimalFraction(new GambleOdds(p, b));

        Assert.Equal(expected, fraction, precision: 10);
    }

    [Theory]
    [InlineData(0.5, 1.0)]
    [InlineData(0.4, 1.0)]
    [InlineData(0.0, 3.0)]
    public void OptimalFraction_WithoutEdge_ReturnsZero(double p, double b)
    {
        Assert.Equal(0, KellyFormula.OptimalFraction(new GambleOdds(p, b)));
    }

    [Fact]
    public void ExpectedLogGrowth_IsMaximisedAtKellyFraction()
    {
        var odds = new GambleOdds(0.6, 1.5);
        var kelly = KellyFormula.OptimalFraction(odds);
        var atKelly = KellyFormula.ExpectedLogGrowth(odds, kelly);

        Assert.True(atKelly > KellyFormula.ExpectedLogGrowth(odds, kelly * 0.5));
        Assert.True(atKelly > KellyFormula.ExpectedLogGrowth(odds, kelly * 1.5));
    }

    [Fact]
    public void ExpectedLogGrowth_WithZeroFraction_IsZero()
    {
        Assert.Equal(0, KellyFormula.ExpectedLogGrowth(new GambleOdds(0.7, 1), 0));
    }
}
