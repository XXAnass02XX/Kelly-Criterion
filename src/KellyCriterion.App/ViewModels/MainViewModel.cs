using CommunityToolkit.Mvvm.ComponentModel;
using KellyCriterion.App.Services;

namespace KellyCriterion.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    public MainViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        _navigation.CurrentViewModelChanged += (_, _) => OnPropertyChanged(nameof(CurrentViewModel));
    }

    public object? CurrentViewModel => _navigation.CurrentViewModel;
}
