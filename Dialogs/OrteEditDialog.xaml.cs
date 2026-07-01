using BibWpf.ViewModels.Dialogs;

namespace BibWpf.Dialogs;

/// <summary>Code-Behind für den Orte-Edit-Dialog.</summary>
public partial class OrteEditDialog : EditDialogBase
{
    public OrteEditDialog(OrteEditDialogViewModel vm)
    {
        InitializeComponent();
        Initialize(vm);
    }
}
