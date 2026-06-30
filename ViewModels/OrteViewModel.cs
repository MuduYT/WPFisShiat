using System.Collections.ObjectModel;
using BibWpf.Data;
using BibWpf.Models;
using Microsoft.EntityFrameworkCore;

namespace BibWpf.ViewModels;

/// <summary>
/// Read-Only-ViewModel für die Ortsliste.
/// Lädt im Konstruktor alle Orte inkl. verknüpfter Verlage, Autor:innen
/// und Bücher (eager-loaded) aus dem per DI injizierten <see cref="LibraryDbContext"/>.
/// </summary>
public partial class OrteViewModel : BaseViewModel
{
    private readonly LibraryDbContext _db;

    public ObservableCollection<Ort> Orte { get; } = new();

    public OrteViewModel(LibraryDbContext db)
    {
        _db = db;
        Title = "Orte";

        Load();
    }

    private void Load()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var daten = _db.Orte
                .AsNoTracking()
                .Include(o => o.Verlage)
                .Include(o => o.Autoren)
                .Include(o => o.Buecher)
                .OrderBy(o => o.Name)
                .ToList();

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
}
