using System.Windows;
using KellyCriterion.Core.Strategies;

namespace KellyCriterion.App.Views;

public partial class StrategySelectionWindow : Window
{
    public IBettingStrategy? SelectedStrategy { get; private set; }

    public StrategySelectionWindow()
    {
        InitializeComponent();
    }

    private void KellyButton_Click(object sender, RoutedEventArgs e) => Select(new KellyBettingStrategy());

    private void HalfKellyButton_Click(object sender, RoutedEventArgs e) => Select(new HalfKellyStrategy());

    private void Select(IBettingStrategy strategy)
    {
        SelectedStrategy = strategy;
        DialogResult = true;
    }
}
