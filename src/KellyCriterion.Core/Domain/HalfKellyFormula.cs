namespace KellyCriterion.Core.Domain;

public class HalfKellyFormula
{
    public static double OptimalFraction(GambleOdds odds)
    {
        ArgumentNullException.ThrowIfNull(odds);
        var fraction = (odds.WinProbability - odds.LossProbability / odds.PayoutRatio)/2;
        return Math.Clamp(fraction, 0, 1);
    }

    public static double ExpectedLogGrowth(GambleOdds odds, double fraction)
    {
        ArgumentNullException.ThrowIfNull(odds);
        ArgumentOutOfRangeException.ThrowIfNegative(fraction);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fraction, 1);

        var winTerm = odds.WinProbability > 0 ? odds.WinProbability * Math.Log(1 + fraction * odds.PayoutRatio) : 0;
        var lossTerm = odds.LossProbability > 0 ? odds.LossProbability * Math.Log(1 - fraction) : 0;
        return winTerm + lossTerm;
    }
}