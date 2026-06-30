using System.Collections.ObjectModel;
using BibWpf.Data;
using BibWpf.Models;
using Microsoft.EntityFrameworkCore;

namespace BibWpf.ViewModels.Dialogs;

/// <summary>
/// Edit-Dialog für einen <see cref="Verlag"/>. FK-Combobox: Ort (optional).
/// </summary>
public partial class VerlageEditDialogViewModel : BaseEditDialogViewModel<Verlag>
{
    public override string Title => Entity.Id == 0 ? "Neuer Verlag" : "Verlag bearbeiten";

    public ObservableCollection<Ort> OrteListe { get; }

    public VerlageEditDialogViewModel(LibraryDbContext db, Verlag? existing)
        : base(db, existing)
    {
        OrteListe = new ObservableCollection<Ort>(
            db.Orte.AsNoTracking().OrderBy(o => o.Name).ToList());
    }
}
