namespace KellyCriterion.Core.Domain;

public sealed record SimulationParameters
{
    public SimulationParameters(int runs, int gamblesPerRun, GambleOdds odds, double initialWealth = 1.0)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(runs);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(runs, SimulationLimits.MaxRuns);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(gamblesPerRun);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(gamblesPerRun, SimulationLimits.MaxGamblesPerRun);
        if ((long)runs * gamblesPerRun > SimulationLimits.MaxTotalGambles)
            throw new ArgumentOutOfRangeException(nameof(runs), runs, $"Runs × gambles must not exceed {SimulationLimits.MaxTotalGambles:N0}.");
        ArgumentNullException.ThrowIfNull(odds);
        if (!double.IsFinite(initialWealth) || initialWealth <= 0)
            throw new ArgumentOutOfRangeException(nameof(initialWealth), initialWealth, "Initial wealth must be a positive number.");

        Runs = runs;
        GamblesPerRun = gamblesPerRun;
        Odds = odds;
        InitialWealth = initialWealth;
    }

    public int Runs { get; }

    public int GamblesPerRun { get; }

    public GambleOdds Odds { get; }

    public double InitialWealth { get; }
}
