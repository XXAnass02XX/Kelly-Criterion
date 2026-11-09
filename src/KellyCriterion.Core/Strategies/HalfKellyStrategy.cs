using KellyCriterion.Core.Domain;
namespace KellyCriterion.Core.Strategies;

public class HalfKellyStrategy : IBettingStrategy
{
    public string Name => "Half Kelly";
    public double FractionToWager(GambleOdds o) => KellyFormula.OptimalFraction(o) / 2;
}