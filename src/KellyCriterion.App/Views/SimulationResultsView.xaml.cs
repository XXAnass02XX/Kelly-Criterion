using System.Windows;
using System.Windows.Controls;

namespace KellyCriterion.App.Views;

public partial class SimulationResultsView : UserControl
{
    public SimulationResultsView() => InitializeComponent();

    private void OnLoaded(object sender, RoutedEventArgs e) => Focus();
}
