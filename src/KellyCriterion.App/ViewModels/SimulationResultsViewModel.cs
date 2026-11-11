using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KellyCriterion.App.Formatting;
using KellyCriterion.App.Services;
using KellyCriterion.Core.Simulation;

namespace KellyCriterion.App.ViewModels;

public sealed partial class SimulationResultsViewModel(SimulationResult result, INavigationService navigation) : ObservableObject
{
    public SimulationResult Result { get; } = result;

    public IReadOnlyList<double[]> WealthPaths => Result.WealthPaths;

    public double[] AverageWealthPath => Result.AverageWealthPath;

    public double[] MedianWealthPath => Result.MedianWealthPath;

    public SimulationSummary Summary => Result.Summary;

    public double InitialWealth => Result.Parameters.InitialWealth;

    public bool HasEdge => Result.WagerFraction > 0;

    public double TheoreticalMedianFinalWealth =>
        InitialWealth * Math.Exp(Result.Parameters.GamblesPerRun * Result.ExpectedLogGrowthPerGamble);

    public string Headline =>
        $"{Result.Parameters.Runs:N0} runs × {Result.Parameters.GamblesPerRun:N0} gambles  ·  " +
        $"p = {Result.Parameters.Odds.WinProbability:0.###}  ·  b = {Result.Parameters.Odds.PayoutRatio:0.###}  ·  " +
        $"initial portfolio = {NumberFormatter.Compact(InitialWealth)}";

    [ObservableProperty]
    public partial bool UseLogScale { get; set; } = true;

    [RelayCommand]
    private void Back() => navigation.ShowSetup();
}
