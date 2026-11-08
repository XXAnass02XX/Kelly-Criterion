using KellyCriterion.Core.Domain;

namespace KellyCriterion.Core.Strategies;

public sealed class KellyBettingStrategy : IBettingStrategy
{
    public string Name => "Kelly Criterion";

    public double FractionToWager(GambleOdds odds) => KellyFormula.OptimalFraction(odds);
}
