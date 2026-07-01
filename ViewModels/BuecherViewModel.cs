using System.Collections.ObjectModel;
using BibWpf.Data;
using BibWpf.Models;
using BibWpf.ViewModels.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace BibWpf.ViewModels;

/// <summary>
/// ViewModel für die Bücherliste (mit CRUD-Funktionalität).
/// </summary>
public partial class BuecherViewModel : BaseViewModel
{
    private readonly LibraryDbContext _db;
    private readonly IDialogService _dialogService;

    /// <summary>ObservableCollection: DataGrid in der View bindet hieran.</summary>
    public ObservableCollection<Buch> Buecher { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private Buch? _selectedItem;

    public bool CanEditOrDelete => SelectedItem != null;

    public BuecherViewModel(LibraryDbContext db, IDialogService dialogService)
    {
        _db = db;
        _dialogService = dialogService;
        Title = "Bücher";
    }

    public async Task ReloadAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var daten = await _db.Buecher
                .Include(b => b.Autor)
                .Include(b => b.Verlag)
                .OrderBy(b => b.Id)
                .ToListAsync();

            Buecher.Clear();
            foreach (var b in daten)
                Buecher.Add(b);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Laden der Bücher: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var result = _dialogService.ShowEditBuch(null);
        await ReloadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task EditAsync()
    {
        if (SelectedItem is null) return;
        var result = _dialogService.ShowEditBuch(SelectedItem);
        await ReloadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteAsync()
    {
        if (SelectedItem is null) return;

        var request = new ConfirmDeleteRequest(
            EntityName: "Buch",
            EntityLabel: SelectedItem.Titel,
            AffectedEntries: Array.Empty<string>(),
            CanCascade: false);

        var result = _dialogService.ShowConfirmDelete(request);
        if (result.Confirmed)
        {
            try
            {
                // Da SelectedItem aus dem parent context geladen wurde (AsNoTracking), müssen wir es im aktuellen context finden/entfernen.
                var trackingEntity = await _db.Buecher.FindAsync(SelectedItem.Id);
                if (trackingEntity is not null)
                {
                    _db.Buecher.Remove(trackingEntity);
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fehler beim Löschen des Buches: {ex.Message}";
            }
            await ReloadAsync();
        }
    }
}
