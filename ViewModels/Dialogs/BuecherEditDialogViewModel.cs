using System.Collections.ObjectModel;
using BibWpf.Data;
using BibWpf.Models;
using Microsoft.EntityFrameworkCore;

namespace BibWpf.ViewModels.Dialogs;

/// <summary>
/// Edit-Dialog für ein <see cref="Buch"/>. Lädt im Konstruktor die Listen
/// für die FK-Comboboxen (Autor/Verlag/Ort).
/// </summary>
public partial class BuecherEditDialogViewModel : BaseEditDialogViewModel<Buch>
{
    public override string Title => Entity.Id == 0 ? "Neues Buch" : "Buch bearbeiten";

    public ObservableCollection<Autor> AutorenListe { get; }
    public ObservableCollection<Verlag> VerlageListe { get; }
    public ObservableCollection<Ort> OrteListe { get; }

    public BuecherEditDialogViewModel(LibraryDbContext db, Buch? existing)
        : base(db, existing)
    {
        AutorenListe = new ObservableCollection<Autor>(
            db.Autoren.AsNoTracking().OrderBy(a => a.Nachname).ThenBy(a => a.Vorname).ToList());
        VerlageListe = new ObservableCollection<Verlag>(
            db.Verlage.AsNoTracking().OrderBy(v => v.Name).ToList());
        OrteListe = new ObservableCollection<Ort>(
            db.Orte.AsNoTracking().OrderBy(o => o.Name).ToList());
    }
}
