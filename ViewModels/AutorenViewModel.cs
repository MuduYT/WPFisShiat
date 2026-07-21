using System.Collections.ObjectModel;
using BibWpf.Data;
using BibWpf.Models;
using BibWpf.ViewModels.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace BibWpf.ViewModels;

/// <summary>
/// ViewModel für die Autorenliste (mit CRUD-Funktionalität).
/// Neu-/Bearbeiten öffnet das rechte Slide-In-Panel.
/// </summary>
public partial class AutorenViewModel : BaseViewModel
{
    private readonly LibraryDbContext _db;
    private readonly IDialogService _dialogService;
    private readonly IEditPanelService _editPanelService;

    public ObservableCollection<Autor> Autoren { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private Autor? _selectedItem;

    public bool CanEditOrDelete => SelectedItem is not null;

    public AutorenViewModel(LibraryDbContext db, IDialogService dialogService, IEditPanelService editPanelService)
    {
        _db = db;
        _dialogService = dialogService;
        _editPanelService = editPanelService;
        Title = "Autor:innen";
    }

    public async Task ReloadAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var daten = await _db.Autoren
                .Include(a => a.Ort)
                .Include(a => a.Buecher)
                .OrderBy(a => a.Id)
                .ToListAsync();

            Autoren.Clear();
            foreach (var a in daten)
                Autoren.Add(a);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Laden der Autor:innen: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Add()
    {
        _editPanelService.ShowEditAutor(null);
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private void Edit()
    {
        if (SelectedItem is null) return;
        _editPanelService.ShowEditAutor(SelectedItem);
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteAsync()
    {
        if (SelectedItem is null) return;

        var trackingEntity = await _db.Autoren
            .Include(a => a.Buecher)
            .FirstOrDefaultAsync(a => a.Id == SelectedItem.Id);

        if (trackingEntity is null) return;

        var affected = trackingEntity.Buecher
            .Select(b => $"Buch: {b.Titel}")
            .ToList();

        var request = new ConfirmDeleteRequest(
            EntityName: "Autor:in",
            EntityLabel: SelectedItem.VollstaendigerName,
            AffectedEntries: affected,
            CanCascade: false);

        var result = _dialogService.ShowConfirmDelete(request);
        if (result.Confirmed)
        {
            try
            {
                _db.Autoren.Remove(trackingEntity);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fehler beim Löschen des Autors: {ex.Message}";
            }
            await ReloadAsync();
        }
    }
}
