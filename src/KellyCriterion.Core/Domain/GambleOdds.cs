namespace KellyCriterion.Core.Domain;

public sealed record GambleOdds
{
    public GambleOdds(double winProbability, double payoutRatio)
    {
        if (double.IsNaN(winProbability) || winProbability is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(winProbability), winProbability, "Win probability must be between 0 and 1.");
        if (!double.IsFinite(payoutRatio) || payoutRatio <= 0)
            throw new ArgumentOutOfRangeException(nameof(payoutRatio), payoutRatio, "Payout ratio must be a positive number.");

        WinProbability = winProbability;
        PayoutRatio = payoutRatio;
    }

    public double WinProbability { get; }

    public double PayoutRatio { get; }

    public double LossProbability => 1 - WinProbability;
}
