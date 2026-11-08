using KellyCriterion.App.ViewModels;
using KellyCriterion.Core.Simulation;
using Microsoft.Extensions.DependencyInjection;

namespace KellyCriterion.App.Services;

public sealed class NavigationService(IServiceProvider services) : INavigationService
{
    public object? CurrentViewModel
    {
        get;
        private set
        {
            if (ReferenceEquals(field, value))
                return;
            field = value;
            CurrentViewModelChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? CurrentViewModelChanged;

    public void ShowSetup() => CurrentViewModel = services.GetRequiredService<SimulationSetupViewModel>();

    public void ShowResults(SimulationResult result) =>
        CurrentViewModel = ActivatorUtilities.CreateInstance<SimulationResultsViewModel>(services, result);
}
