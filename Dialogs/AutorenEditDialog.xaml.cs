using BibWpf.ViewModels.Dialogs;

namespace BibWpf.Dialogs;

/// <summary>Code-Behind für den Autor:innen-Edit-Dialog.</summary>
public partial class AutorenEditDialog : EditDialogBase
{
    public AutorenEditDialog(AutorenEditDialogViewModel vm)
    {
        InitializeComponent();
        Initialize(vm);
    }
}
