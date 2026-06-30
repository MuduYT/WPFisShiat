using System.Collections.ObjectModel;
using BibWpf.Data;
using BibWpf.Models;
using Microsoft.EntityFrameworkCore;

namespace BibWpf.ViewModels;

/// <summary>
/// Read-Only-ViewModel für die Verlagsliste.
/// Lädt im Konstruktor alle Verlage inkl. Sitz und Bücher (eager-loaded)
/// aus dem per DI injizierten <see cref="LibraryDbContext"/>.
/// </summary>
public partial class VerlageViewModel : BaseViewModel
{
    private readonly LibraryDbContext _db;

    public ObservableCollection<Verlag> Verlage { get; } = new();

    public VerlageViewModel(LibraryDbContext db)
    {
        _db = db;
        Title = "Verlage";

        Load();
    }

    private void Load()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var daten = _db.Verlage
                .AsNoTracking()
                .Include(v => v.Ort)
                .Include(v => v.Buecher)
                .OrderBy(v => v.Name)
                .ToList();

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
}
