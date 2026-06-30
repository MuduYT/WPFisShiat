using System.Collections.ObjectModel;
using BibWpf.Data;
using BibWpf.Models;
using Microsoft.EntityFrameworkCore;

namespace BibWpf.ViewModels;

/// <summary>
/// Read-Only-ViewModel für die Autorenliste.
/// Lädt im Konstruktor alle Autor:innen inkl. Wohnort und Bücher (eager-loaded)
/// aus dem per DI injizierten <see cref="LibraryDbContext"/>.
/// </summary>
public partial class AutorenViewModel : BaseViewModel
{
    private readonly LibraryDbContext _db;

    public ObservableCollection<Autor> Autoren { get; } = new();

    public AutorenViewModel(LibraryDbContext db)
    {
        _db = db;
        Title = "Autor:innen";

        Load();
    }

    private void Load()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var daten = _db.Autoren
                .AsNoTracking()
                .Include(a => a.Ort)
                .Include(a => a.Buecher)
                .OrderBy(a => a.Nachname)
                .ThenBy(a => a.Vorname)
                .ToList();

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
}
