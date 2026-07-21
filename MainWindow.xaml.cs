using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BibWpf.ViewModels;

namespace BibWpf;

/// <summary>
/// Hauptfenster mit dem rechten, animierten Slide-In-Edit-Panel.
/// Das Panel liegt als Overlay über dem gesamten Fenster und ist 50 % breit.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;
    private bool _isClosingPanel;

    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        _mainViewModel = mainViewModel;
        DataContext = mainViewModel;
        _mainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        Closed += OnClosed;
    }

    private void OnRootGridSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var panelWidth = GetPanelWidth();
        if (panelWidth <= 0)
            return;

        EditPanel.Width = panelWidth;
        if (!_mainViewModel.IsEditPanelOpen)
            ((TranslateTransform)EditPanel.RenderTransform).X = panelWidth;
    }

    private double GetPanelWidth()
        => RootGrid.ActualWidth > 0 ? RootGrid.ActualWidth * 0.5 : 0;

    private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.IsEditPanelOpen))
            return;

        if (Dispatcher.CheckAccess())
            UpdateEditPanel(_mainViewModel.IsEditPanelOpen);
        else
            Dispatcher.Invoke(() => UpdateEditPanel(_mainViewModel.IsEditPanelOpen));
    }

    private void OnOverlayMouseDown(object sender, MouseButtonEventArgs e)
    {
        _mainViewModel.CloseEditCommand?.Execute(null);
    }

    private void UpdateEditPanel(bool isOpen)
    {
        var panelWidth = GetPanelWidth();
        if (panelWidth <= 0)
            return;

        EditPanel.Width = panelWidth;

        if (isOpen)
        {
            _isClosingPanel = false;
            Overlay.Visibility = Visibility.Visible;
            EditPanel.Visibility = Visibility.Visible;

            var transform = (TranslateTransform)EditPanel.RenderTransform;
            transform.BeginAnimation(TranslateTransform.XProperty, null);
            transform.X = panelWidth;

            var animation = new DoubleAnimation
            {
                From = panelWidth,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            transform.BeginAnimation(TranslateTransform.XProperty, animation);
            return;
        }

        if (_isClosingPanel || EditPanel.Visibility != Visibility.Visible)
            return;

        _isClosingPanel = true;
        var closeTransform = (TranslateTransform)EditPanel.RenderTransform;
        closeTransform.BeginAnimation(TranslateTransform.XProperty, null);

        var closeAnimation = new DoubleAnimation
        {
            From = closeTransform.X,
            To = panelWidth,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        closeAnimation.Completed += (_, _) =>
        {
            EditPanel.Visibility = Visibility.Collapsed;
            Overlay.Visibility = Visibility.Collapsed;
            closeTransform.X = GetPanelWidth();
            _isClosingPanel = false;
        };
        closeTransform.BeginAnimation(TranslateTransform.XProperty, closeAnimation);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _mainViewModel.PropertyChanged -= OnMainViewModelPropertyChanged;
        Closed -= OnClosed;
    }
}
