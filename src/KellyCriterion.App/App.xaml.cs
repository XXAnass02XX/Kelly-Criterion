using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using KellyCriterion.App.Services;
using KellyCriterion.App.ViewModels;
using KellyCriterion.App.Views;
using KellyCriterion.Core.Randomness;
using KellyCriterion.Core.Simulation;
using Microsoft.Extensions.DependencyInjection;

namespace KellyCriterion.App;

public partial class App : Application
{
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        _services = ConfigureServices();
        _services.GetRequiredService<INavigationService>().ShowSetup();

        MainWindow = _services.GetRequiredService<MainWindow>();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Trace.TraceError(e.Exception.ToString());
        MessageBox.Show(e.Exception.Message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private static ServiceProvider ConfigureServices() =>
        new ServiceCollection()
            .AddSingleton<IRandomSourceFactory>(_ => new SystemRandomSourceFactory())
            .AddSingleton<IGamblerSimulator, GamblerSimulator>()
            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<MainViewModel>()
            .AddSingleton<SimulationSetupViewModel>()
            .AddSingleton<MainWindow>()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
}
