using KellyCriterion.Core.Simulation;

namespace KellyCriterion.App.Services;

public interface INavigationService
{
    object? CurrentViewModel { get; }

    event EventHandler? CurrentViewModelChanged;

    void ShowSetup();

    void ShowResults(SimulationResult result);
}
