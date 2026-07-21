using BibWpf.Models;

namespace BibWpf.ViewModels;

/// <summary>
/// Koordinator für das rechte Slide-In-Edit-Panel.
/// Sub-ViewModels feuern Events; <see cref="MainViewModel"/> abonniert diese
/// und steuert das Panel. So entsteht keine zirkuläre DI-Abhängigkeit.
/// </summary>
public interface IEditPanelService
{
    event EventHandler<Buch?>? ShowEditBuchRequested;
    event EventHandler<Autor?>? ShowEditAutorRequested;
    event EventHandler<Verlag?>? ShowEditVerlagRequested;
    event EventHandler<Ort?>? ShowEditOrtRequested;
    event EventHandler? CloseEditRequested;

    void ShowEditBuch(Buch? entity);
    void ShowEditAutor(Autor? entity);
    void ShowEditVerlag(Verlag? entity);
    void ShowEditOrt(Ort? entity);
    void CloseEdit();
}

/// <summary>
/// Standard-Implementierung von <see cref="IEditPanelService"/>.
/// </summary>
public sealed class EditPanelService : IEditPanelService
{
    public event EventHandler<Buch?>? ShowEditBuchRequested;
    public event EventHandler<Autor?>? ShowEditAutorRequested;
    public event EventHandler<Verlag?>? ShowEditVerlagRequested;
    public event EventHandler<Ort?>? ShowEditOrtRequested;
    public event EventHandler? CloseEditRequested;

    public void ShowEditBuch(Buch? entity) => ShowEditBuchRequested?.Invoke(this, entity);
    public void ShowEditAutor(Autor? entity) => ShowEditAutorRequested?.Invoke(this, entity);
    public void ShowEditVerlag(Verlag? entity) => ShowEditVerlagRequested?.Invoke(this, entity);
    public void ShowEditOrt(Ort? entity) => ShowEditOrtRequested?.Invoke(this, entity);
    public void CloseEdit() => CloseEditRequested?.Invoke(this, EventArgs.Empty);
}
