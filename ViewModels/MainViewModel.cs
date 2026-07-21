using BibWpf.Models;
using BibWpf.ViewModels.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace BibWpf.ViewModels;

/// <summary>
/// Haupt-ViewModel. Hält die aktuell angezeigte Sub-View, stellt
/// RelayCommands für die Sidebar-Navigation bereit und steuert das
/// rechte Slide-In-Edit-Panel.
/// </summary>
public partial class MainViewModel : BaseViewModel
{
    private readonly BuecherViewModel _buecherVm;
    private readonly AutorenViewModel _autorenVm;
    private readonly VerlageViewModel _verlageVm;
    private readonly OrteViewModel _orteVm;
    private readonly SettingsViewModel _settingsVm;
    private readonly IEditPanelService _editPanelService;
    private readonly IServiceScopeFactory _scopeFactory;
    private IServiceScope? _editScope;

    [ObservableProperty]
    private BaseViewModel? _currentView;

    /// <summary>ViewModel des rechten Slide-In-Edit-Panels.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditPanelOpen))]
    private IBaseEditDialogViewModel? _currentEditViewModel;

    public bool IsEditPanelOpen => CurrentEditViewModel is not null;

    public MainViewModel(
        BuecherViewModel buecherVm,
        AutorenViewModel autorenVm,
        VerlageViewModel verlageVm,
        OrteViewModel orteVm,
        SettingsViewModel settingsVm,
        IEditPanelService editPanelService,
        IServiceScopeFactory scopeFactory)
    {
        _buecherVm = buecherVm;
        _autorenVm = autorenVm;
        _verlageVm = verlageVm;
        _orteVm = orteVm;
        _settingsVm = settingsVm;
        _editPanelService = editPanelService;
        _scopeFactory = scopeFactory;

        _editPanelService.ShowEditBuchRequested += (_, entity) => OpenEdit<Buch, BuecherEditDialogViewModel>(entity);
        _editPanelService.ShowEditAutorRequested += (_, entity) => OpenEdit<Autor, AutorenEditDialogViewModel>(entity);
        _editPanelService.ShowEditVerlagRequested += (_, entity) => OpenEdit<Verlag, VerlageEditDialogViewModel>(entity);
        _editPanelService.ShowEditOrtRequested += (_, entity) => OpenEdit<Ort, OrteEditDialogViewModel>(entity);
        _editPanelService.CloseEditRequested += (_, _) => CloseEdit();

        Title = "Bibliothek";
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

    [RelayCommand]
    private void CloseEdit()
    {
        if (CurrentEditViewModel is not null)
        {
            CurrentEditViewModel.RequestClose -= OnEditRequestClose;
            CurrentEditViewModel = null;
        }

        _editScope?.Dispose();
        _editScope = null;
    }

    private void OpenEdit<TEntity, TVM>(TEntity? entity)
        where TEntity : class, new()
        where TVM : BaseEditDialogViewModel<TEntity>
    {
        CloseEdit();

        // Der Scope bleibt offen, solange das Panel sichtbar ist. Dadurch bleibt
        // auch der injizierte DbContext für Speichern und Validierung gültig.
        _editScope = _scopeFactory.CreateScope();
        var vm = ActivatorUtilities.CreateInstance<TVM>(
            _editScope.ServiceProvider,
            entity ?? new TEntity());

        vm.RequestClose += OnEditRequestClose;
        CurrentEditViewModel = vm;
    }

    private async void OnEditRequestClose(object? sender, EditDialogResultEventArgs e)
    {
        CloseEdit();

        if (CurrentView is not null)
        {
            var reloadMethod = CurrentView.GetType().GetMethod("ReloadAsync");
            if (reloadMethod?.Invoke(CurrentView, null) is Task task)
                await task;
        }
    }
}
