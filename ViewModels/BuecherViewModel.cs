using System.Collections.ObjectModel;
using BibWpf.Data;
using BibWpf.Models;
using Microsoft.EntityFrameworkCore;

namespace BibWpf.ViewModels;

/// <summary>
/// Read-Only-ViewModel für die Bücherliste.
/// Lädt im Konstruktor alle Bücher inkl. Autor/Verlag/Ort (eager-loaded)
/// aus dem per DI injizierten <see cref="LibraryDbContext"/>.
/// </summary>
public partial class BuecherViewModel : BaseViewModel
{
    private readonly LibraryDbContext _db;

    /// <summary>ObservableCollection: DataGrid in der View bindet hieran.</summary>
    public ObservableCollection<Buch> Buecher { get; } = new();

    public BuecherViewModel(LibraryDbContext db)
    {
        _db = db;
        Title = "Bücher";

        Load();
    }

    /// <summary>
    /// Synchrone Initialbefüllung. Später (Schritt 3) ersetzt durch async/Reload.
    /// </summary>
    private void Load()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var daten = _db.Buecher
                .AsNoTracking()
                .Include(b => b.Autor)
                .Include(b => b.Verlag)
                .Include(b => b.Ort)
                .OrderBy(b => b.Titel)
                .ToList();

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
}
