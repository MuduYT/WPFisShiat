using System.Collections.ObjectModel;
using BibWpf.Data;
using BibWpf.Models;
using BibWpf.ViewModels.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace BibWpf.ViewModels;

/// <summary>
/// ViewModel für die Ortsliste (mit CRUD-Funktionalität).
/// Neu-/Bearbeiten öffnet das rechte Slide-In-Panel.
/// </summary>
public partial class OrteViewModel : BaseViewModel
{
    private readonly LibraryDbContext _db;
    private readonly IDialogService _dialogService;
    private readonly IEditPanelService _editPanelService;

    public ObservableCollection<Ort> Orte { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private Ort? _selectedItem;

    public bool CanEditOrDelete => SelectedItem is not null;

    public OrteViewModel(LibraryDbContext db, IDialogService dialogService, IEditPanelService editPanelService)
    {
        _db = db;
        _dialogService = dialogService;
        _editPanelService = editPanelService;
        Title = "Orte";
    }

    public async Task ReloadAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var daten = await _db.Orte
                .Include(o => o.Verlage)
                .Include(o => o.Autoren)
                .OrderBy(o => o.Id)
                .ToListAsync();

            Orte.Clear();
            foreach (var o in daten)
                Orte.Add(o);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Laden der Orte: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Add()
    {
        _editPanelService.ShowEditOrt(null);
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private void Edit()
    {
        if (SelectedItem is null) return;
        _editPanelService.ShowEditOrt(SelectedItem);
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteAsync()
    {
        if (SelectedItem is null) return;

        var trackingEntity = await _db.Orte
            .Include(o => o.Verlage)
            .Include(o => o.Autoren)
            .FirstOrDefaultAsync(o => o.Id == SelectedItem.Id);

        if (trackingEntity is null) return;

        var affected = new List<string>();
        foreach (var v in trackingEntity.Verlage)
            affected.Add($"Verlag: {v.Name}");
        foreach (var a in trackingEntity.Autoren)
            affected.Add($"Autor: {a.VollstaendigerName}");

        var request = new ConfirmDeleteRequest(
            EntityName: "Ort",
            EntityLabel: SelectedItem.ToString(),
            AffectedEntries: affected,
            CanCascade: true);

        var result = _dialogService.ShowConfirmDelete(request);
        if (result.Confirmed)
        {
            try
            {
                _db.Orte.Remove(trackingEntity);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fehler beim Löschen des Ortes: {ex.Message}";
            }
            await ReloadAsync();
        }
    }
}
