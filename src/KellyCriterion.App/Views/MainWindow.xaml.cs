using System.Windows;
using KellyCriterion.App.ViewModels;

namespace KellyCriterion.App.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
