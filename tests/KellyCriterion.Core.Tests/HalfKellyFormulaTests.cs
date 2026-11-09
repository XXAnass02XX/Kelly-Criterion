using KellyCriterion.Core.Domain;

namespace KellyCriterion.Core.Tests;

public class HalfKellyFormulaTests
{
    [Theory]
    [InlineData(0.6, 1.0, 0.1)]
    public void OptimalFraction_ReturnsHalfKellyFraction(double p, double b, double expected)
    {
        var fraction = HalfKellyFormula.OptimalFraction(new GambleOdds(p, b));

        Assert.Equal(expected, fraction, precision: 10);
    }

    [Theory]
    [InlineData(0.5, 1.0)]
    public void OptimalFraction_WithoutEdge_ReturnsZero(double p, double b)
    {
        Assert.Equal(0, HalfKellyFormula.OptimalFraction(new GambleOdds(p, b)));
    }

}