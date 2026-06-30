using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BibWpf.ViewModels;

/// <summary>
/// Haupt-ViewModel. Hält die aktuell angezeigte Sub-View und stellt
/// RelayCommands für die Sidebar-Navigation bereit. Die 4 Sub-ViewModels
/// werden per DI injiziert. Die Auflösung Sub-VM → UserControl passiert
/// über DataTemplates in MainWindow.xaml.
/// </summary>
public partial class MainViewModel : BaseViewModel
{
    private readonly BuecherViewModel _buecherVm;
    private readonly AutorenViewModel _autorenVm;
    private readonly VerlageViewModel _verlageVm;
    private readonly OrteViewModel _orteVm;

    /// <summary>
    /// Aktuell im <c>ContentControl</c> angezeigte Sub-View (vom Typ <see cref="BaseViewModel"/>).
    /// Wird per DataTemplate in MainWindow.xaml auf die passende UserControl gemappt.
    /// </summary>
    [ObservableProperty]
    private BaseViewModel? _currentView;

    public MainViewModel(
        BuecherViewModel buecherVm,
        AutorenViewModel autorenVm,
        VerlageViewModel verlageVm,
        OrteViewModel orteVm)
    {
        _buecherVm = buecherVm;
        _autorenVm = autorenVm;
        _verlageVm = verlageVm;
        _orteVm = orteVm;

        Title = "Bibliothek";

        // Standardansicht beim Start
        CurrentView = _buecherVm;
    }

    [RelayCommand]
    private void ShowBuecher() => CurrentView = _buecherVm;

    [RelayCommand]
    private void ShowAutoren() => CurrentView = _autorenVm;

    [RelayCommand]
    private void ShowVerlage() => CurrentView = _verlageVm;

    [RelayCommand]
    private void ShowOrte() => CurrentView = _orteVm;
}
