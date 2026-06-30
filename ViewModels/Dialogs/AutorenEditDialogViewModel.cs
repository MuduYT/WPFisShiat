using System.Collections.ObjectModel;
using BibWpf.Data;
using BibWpf.Models;
using Microsoft.EntityFrameworkCore;

namespace BibWpf.ViewModels.Dialogs;

/// <summary>
/// Edit-Dialog für eine:n <see cref="Autor"/>. FK-Combobox: Ort (optional).
/// </summary>
public partial class AutorenEditDialogViewModel : BaseEditDialogViewModel<Autor>
{
    public override string Title => Entity.Id == 0 ? "Neue:r Autor:in" : "Autor:in bearbeiten";

    public ObservableCollection<Ort> OrteListe { get; }

    public AutorenEditDialogViewModel(LibraryDbContext db, Autor? existing)
        : base(db, existing)
    {
        OrteListe = new ObservableCollection<Ort>(
            db.Orte.AsNoTracking().OrderBy(o => o.Name).ToList());
    }
}
