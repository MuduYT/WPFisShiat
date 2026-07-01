using System.Windows;
using BibWpf.ViewModels.Dialogs;

namespace BibWpf.Dialogs;

/// <summary>
/// Gemeinsame Code-Behind-Basis für alle 4 Edit-Dialog-Fenster.
/// Triggert automatisch <see cref="BaseEditDialogViewModel{T}.ValidateAll"/>
/// beim Laden, und reagiert auf <see cref="BaseEditDialogViewModel{T}.RequestClose"/>,
/// um das Fenster modal zu schließen.
/// </summary>
/// <remarks>
/// Da die VMs generisch über der Entität parametrisiert sind, akzeptiert diese
/// Basis bewusst <c>dynamic</c> für das VM-Argument — das ist hier unbedenklich,
/// weil das VM-Event nur ein <see cref="EditDialogResult"/> liefert und das VM
/// über das <see cref="DataContext"/> sauber typsicher an die View gebunden ist.
/// </remarks>
public abstract class EditDialogBase : Window
{
    protected void Initialize(IBaseEditDialogViewModel vm)
    {
        DataContext = vm;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 480;
        MinHeight = 320;
        ShowInTaskbar = false;

        Loaded += (_, _) =>
        {
            // Validierung initial anstoßen, damit HasErrors korrekt ist.
            vm.ValidateAll();
            // RequestClose-Event abonnieren.
            vm.RequestClose -= OnRequestClose;
            vm.RequestClose += OnRequestClose;
        };
    }

    private void OnRequestClose(object? sender, EditDialogResultEventArgs e)
    {
        DialogResult = e.Result == EditDialogResult.Saved;
        Close();
    }
}
