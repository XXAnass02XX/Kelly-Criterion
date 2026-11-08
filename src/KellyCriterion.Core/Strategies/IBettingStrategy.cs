using KellyCriterion.Core.Domain;

namespace KellyCriterion.Core.Strategies;

public interface IBettingStrategy
{
    string Name { get; }

    double FractionToWager(GambleOdds odds);
}
