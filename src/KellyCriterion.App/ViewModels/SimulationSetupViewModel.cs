using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KellyCriterion.App.Input;
using KellyCriterion.App.Services;
using KellyCriterion.Core.Domain;
using KellyCriterion.Core.Simulation;
using KellyCriterion.Core.Strategies;

namespace KellyCriterion.App.ViewModels;

public sealed partial class SimulationSetupViewModel : ObservableValidator
{
    private readonly IGamblerSimulator _simulator;
    private readonly INavigationService _navigation;

    public SimulationSetupViewModel(IGamblerSimulator simulator, INavigationService navigation)
    {
        _simulator = simulator;
        _navigation = navigation;

        Runs = "200";
        Gambles = "1000";
        WinProbability = "0.6";
        PayoutRatio = "1";
        InitialWealth = "100";
        SelectedStrategy = Strategies[0];

        ErrorsChanged += (_, _) =>
        {
            SimulateCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(KellyFractionPreview));
        };
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(SimulationSetupViewModel), nameof(ValidateRuns))]
    public partial string Runs { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(SimulationSetupViewModel), nameof(ValidateGambles))]
    public partial string Gambles { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(KellyFractionPreview))]
    [CustomValidation(typeof(SimulationSetupViewModel), nameof(ValidateWinProbability))]
    public partial string WinProbability { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(KellyFractionPreview))]
    [CustomValidation(typeof(SimulationSetupViewModel), nameof(ValidatePayoutRatio))]
    public partial string PayoutRatio { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(SimulationSetupViewModel), nameof(ValidateInitialWealth))]
    public partial string InitialWealth { get; set; }

    public IReadOnlyList<IBettingStrategy> Strategies { get; } = [new KellyBettingStrategy(), new HalfKellyStrategy()];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(KellyFractionPreview))]
    public partial IBettingStrategy SelectedStrategy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public double? KellyFractionPreview => TryCreateOdds(out var odds) ? SelectedStrategy.FractionToWager(odds) : null;

    public static ValidationResult? ValidateRuns(string value, ValidationContext context) =>
        ValidateCount(value, SimulationLimits.MaxRuns, "runs", context);

    public static ValidationResult? ValidateGambles(string value, ValidationContext context) =>
        ValidateCount(value, SimulationLimits.MaxGamblesPerRun, "gambles", context);

    public static ValidationResult? ValidateWinProbability(string value, ValidationContext context) =>
        NumberParser.TryParseDouble(value, out var p) && p is >= 0 and <= 1
            ? ValidationResult.Success
            : new ValidationResult("Enter a probability between 0 and 1.");

    public static ValidationResult? ValidatePayoutRatio(string value, ValidationContext context) =>
        NumberParser.TryParseDouble(value, out var b) && b > 0
            ? ValidationResult.Success
            : new ValidationResult("Enter a ratio greater than 0.");

    public static ValidationResult? ValidateInitialWealth(string value, ValidationContext context) =>
        NumberParser.TryParseDouble(value, out var wealth) && wealth > 0
            ? ValidationResult.Success
            : new ValidationResult("Enter an initial portfolio value greater than 0.");

    [RelayCommand(CanExecute = nameof(CanSimulate), IncludeCancelCommand = true)]
    private async Task SimulateAsync(CancellationToken cancellationToken)
    {
        ValidateAllProperties();
        if (HasErrors || !TryCreateParameters(out var parameters))
            return;

        ErrorMessage = null;
        try
        {
            var result = await Task.Run(() => _simulator.Simulate(parameters, cancellationToken), cancellationToken);
            _navigation.ShowResults(result);
        }
        catch (OperationCanceledException)
        {
            ErrorMessage = "Simulation cancelled.";
        }
        catch (OutOfMemoryException)
        {
            ErrorMessage = "Not enough memory for this simulation. Reduce the number of runs or gambles.";
        }
    }

    private bool CanSimulate() => !HasErrors;

    partial void OnRunsChanged(string value) => ValidateProperty(Gambles, nameof(Gambles));

    partial void OnGamblesChanged(string value) => ValidateProperty(Runs, nameof(Runs));

    private static ValidationResult? ValidateCount(string value, int max, string noun, ValidationContext context)
    {
        if (!NumberParser.TryParseInt(value, out var count) || count < 1 || count > max)
            return new ValidationResult($"Enter a whole number of {noun} between 1 and {max:N0}.");

        var vm = (SimulationSetupViewModel)context.ObjectInstance;
        if (NumberParser.TryParseInt(vm.Runs, out var runs) && NumberParser.TryParseInt(vm.Gambles, out var gambles)
            && (long)runs * gambles > SimulationLimits.MaxTotalGambles)
            return new ValidationResult($"Runs × gambles must not exceed {SimulationLimits.MaxTotalGambles:N0}.");

        return ValidationResult.Success;
    }

    private bool TryCreateOdds(out GambleOdds odds)
    {
        odds = null!;
        if (!NumberParser.TryParseDouble(WinProbability, out var p) || p is < 0 or > 1
            || !NumberParser.TryParseDouble(PayoutRatio, out var b) || b <= 0)
            return false;

        odds = new GambleOdds(p, b);
        return true;
    }

    private bool TryCreateParameters(out SimulationParameters parameters)
    {
        parameters = null!;
        if (!NumberParser.TryParseInt(Runs, out var runs) || !NumberParser.TryParseInt(Gambles, out var gambles)
            || !NumberParser.TryParseDouble(InitialWealth, out var initialWealth) || !TryCreateOdds(out var odds))
            return false;

        parameters = new SimulationParameters(runs, gambles, odds, SelectedStrategy, initialWealth);
        return true;
    }
}
