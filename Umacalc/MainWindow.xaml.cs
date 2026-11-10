using System.Windows;
using Umacalc.ViewModels;

namespace Umacalc;

public partial class MainWindow : Window
{
    private double _previousWidth = 1100;
    private double _previousHeight = 820;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void SwitchToCompact_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            _previousWidth = ActualWidth > 0 ? ActualWidth : Width;
            _previousHeight = ActualHeight > 0 ? ActualHeight : Height;
            vm.IsCompactMode = true;
            MinWidth = 520;
            MinHeight = 120;
            Width = 660;
            Height = 130;
        }
    }

    private void SwitchToFull_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.IsCompactMode = false;
            MinWidth = 920;
            MinHeight = 680;
            Width = Math.Max(920, _previousWidth);
            Height = Math.Max(680, _previousHeight);
        }
    }
}
