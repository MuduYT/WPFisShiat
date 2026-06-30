using BibWpf.ViewModels.Dialogs;

namespace BibWpf.Dialogs;

/// <summary>Code-Behind für den Buch-Edit-Dialog.</summary>
public partial class BuecherEditDialog : EditDialogBase
{
    public BuecherEditDialog(BuecherEditDialogViewModel vm)
    {
        InitializeComponent();
        Initialize(vm);
    }
}
