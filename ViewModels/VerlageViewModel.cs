using System.Collections.ObjectModel;
using BibWpf.Data;
using BibWpf.Models;
using BibWpf.ViewModels.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace BibWpf.ViewModels;

/// <summary>
/// ViewModel für die Verlagsliste (mit CRUD-Funktionalität).
/// </summary>
public partial class VerlageViewModel : BaseViewModel
{
    private readonly LibraryDbContext _db;
    private readonly IDialogService _dialogService;

    public ObservableCollection<Verlag> Verlage { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private Verlag? _selectedItem;

    public bool CanEditOrDelete => SelectedItem is not null;

    public VerlageViewModel(LibraryDbContext db, IDialogService dialogService)
    {
        _db = db;
        _dialogService = dialogService;
        Title = "Verlage";
    }

    public async Task ReloadAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var daten = await _db.Verlage
                .Include(v => v.Ort)
                .Include(v => v.Buecher)
                .OrderBy(v => v.Id)
                .ToListAsync();

            Verlage.Clear();
            foreach (var v in daten)
                Verlage.Add(v);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Laden der Verlage: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var result = _dialogService.ShowEditVerlag(null);
        await ReloadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task EditAsync()
    {
        if (SelectedItem is null) return;
        var result = _dialogService.ShowEditVerlag(SelectedItem);
        await ReloadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteAsync()
    {
        if (SelectedItem is null) return;

        // Eager load Buecher to see if this verlag has books referencing them
        var trackingEntity = await _db.Verlage
            .Include(v => v.Buecher)
            .FirstOrDefaultAsync(v => v.Id == SelectedItem.Id);

        if (trackingEntity is null) return;

        var affected = trackingEntity.Buecher
            .Select(b => $"Buch: {b.Titel}")
            .ToList();

        var request = new ConfirmDeleteRequest(
            EntityName: "Verlag",
            EntityLabel: SelectedItem.Name,
            AffectedEntries: affected,
            CanCascade: false); // Restrict constraint

        var result = _dialogService.ShowConfirmDelete(request);
        if (result.Confirmed)
        {
            try
            {
                _db.Verlage.Remove(trackingEntity);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fehler beim Löschen des Verlags: {ex.Message}";
            }
            await ReloadAsync();
        }
    }
}
