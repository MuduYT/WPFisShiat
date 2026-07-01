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
    private readonly SettingsViewModel _settingsVm;

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
        OrteViewModel orteVm,
        SettingsViewModel settingsVm)
    {
        _buecherVm = buecherVm;
        _autorenVm = autorenVm;
        _verlageVm = verlageVm;
        _orteVm = orteVm;
        _settingsVm = settingsVm;

        Title = "Bibliothek";

        // Standardansicht beim Start — lädt Bücher sofort
        CurrentView = _buecherVm;
        _ = _buecherVm.ReloadAsync();
    }

    [RelayCommand]
    private async Task ShowBuecher()
    {
        CurrentView = _buecherVm;
        await _buecherVm.ReloadAsync();
    }

    [RelayCommand]
    private async Task ShowAutoren()
    {
        CurrentView = _autorenVm;
        await _autorenVm.ReloadAsync();
    }

    [RelayCommand]
    private async Task ShowVerlage()
    {
        CurrentView = _verlageVm;
        await _verlageVm.ReloadAsync();
    }

    [RelayCommand]
    private async Task ShowOrte()
    {
        CurrentView = _orteVm;
        await _orteVm.ReloadAsync();
    }

    [RelayCommand]
    private void ShowSettings()
    {
        CurrentView = _settingsVm;
    }
}
