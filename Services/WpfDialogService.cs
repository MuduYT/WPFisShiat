using BibWpf.Data;
using BibWpf.Dialogs;
using BibWpf.Models;
using BibWpf.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BibWpf.Services;

/// <summary>
/// WPF-Implementierung von <see cref="IDialogService"/>. Erzeugt die
/// Edit-Dialog-Windows aus dem DI-Container und zeigt sie modal.
/// </summary>
public sealed class WpfDialogService : IDialogService
{
    private readonly IServiceProvider _sp;

    public WpfDialogService(IServiceProvider sp) => _sp = sp;

    public EditDialogResult ShowEditBuch(Buch? entity)      => ShowEditWindow<BuecherEditDialog, Buch, BuecherEditDialogViewModel>(entity);
    public EditDialogResult ShowEditAutor(Autor? entity)    => ShowEditWindow<AutorenEditDialog, Autor, AutorenEditDialogViewModel>(entity);
    public EditDialogResult ShowEditVerlag(Verlag? entity)  => ShowEditWindow<VerlageEditDialog, Verlag, VerlageEditDialogViewModel>(entity);
    public EditDialogResult ShowEditOrt(Ort? entity)        => ShowEditWindow<OrteEditDialog, Ort, OrteEditDialogViewModel>(entity);

    /// <summary>
    /// Erzeugt einen frischen DI-Scope (damit der DbContext darin neu ist),
    /// resolved das VM, baut das Window, hängt es an das aktive MainWindow und
    /// zeigt es modal.
    /// </summary>
    private EditDialogResult ShowEditWindow<TWindow, TEntity, TVM>(TEntity? entity)
        where TWindow : EditDialogBase
        where TEntity : class, new()
        where TVM : BaseEditDialogViewModel<TEntity>
    {
        using var scope = _sp.CreateScope();
        var vm = ActivatorUtilities.CreateInstance<TVM>(scope.ServiceProvider, entity ?? new TEntity());
        var window = (TWindow)Activator.CreateInstance(typeof(TWindow), vm)!;
        window.Owner = System.Windows.Application.Current.MainWindow;
        var result = window.ShowDialog();
        return result == true ? EditDialogResult.Saved : EditDialogResult.Cancel;
    }

    public ConfirmDeleteResult ShowConfirmDelete(ConfirmDeleteRequest request)
    {
        // ConfirmDeleteWindow braucht keinen DbContext — kann transient aus dem Root-Scope
        // erzeugt werden, oder direkt mit `new` (es ist VM-frei).
        var window = new ConfirmDeleteWindow(request);
        window.Owner = System.Windows.Application.Current.MainWindow;
        var result = window.ShowDialog();
        return new ConfirmDeleteResult(
            Confirmed: result == true,
            Cascade: window.UserRequestedCascade);
    }
}
