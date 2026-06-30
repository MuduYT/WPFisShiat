using BibWpf.ViewModels.Dialogs;

namespace BibWpf.Dialogs;

/// <summary>Code-Behind für den Verlag-Edit-Dialog.</summary>
public partial class VerlageEditDialog : EditDialogBase
{
    public VerlageEditDialog(VerlageEditDialogViewModel vm)
    {
        InitializeComponent();
        Initialize(vm);
    }
}
