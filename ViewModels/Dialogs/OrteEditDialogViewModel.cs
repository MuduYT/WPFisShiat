using BibWpf.Data;
using BibWpf.Models;

namespace BibWpf.ViewModels.Dialogs;

/// <summary>
/// Edit-Dialog für einen <see cref="Ort"/>. Hat keine FK-Comboboxen
/// (nur eigene Felder Name/Land).
/// </summary>
public partial class OrteEditDialogViewModel : BaseEditDialogViewModel<Ort>
{
    public override string Title => Entity.Id == 0 ? "Neuer Ort" : "Ort bearbeiten";

    public OrteEditDialogViewModel(LibraryDbContext db, Ort? existing)
        : base(db, existing)
    {
    }
}
