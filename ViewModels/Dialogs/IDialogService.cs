using BibWpf.Models;

namespace BibWpf.ViewModels.Dialogs;

/// <summary>
/// Abstraktion für modale Dialoge. Erlaubt es Sub-ViewModels, einen
/// Edit-Dialog oder Confirm-Delete anzuzeigen, ohne direkt von
/// WPF-Window-Klassen abzuhängen.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Zeigt einen Edit-Dialog für <paramref name="entity"/> (Add, wenn <c>null</c>).
    /// </summary>
    /// <returns>
    /// <c>EditDialogResult.Saved</c> bei erfolgreicher Speicherung, sonst <c>Cancel</c>.
    /// </returns>
    EditDialogResult ShowEditBuch(Buch? entity);
    EditDialogResult ShowEditAutor(Autor? entity);
    EditDialogResult ShowEditVerlag(Verlag? entity);
    EditDialogResult ShowEditOrt(Ort? entity);

    /// <summary>
    /// Zeigt den Two-Stage-Confirm-Delete-Dialog und liefert
    /// die Antwort des Users (Delete ja / nein, ggf. mit SetNull-Information).
    /// </summary>
    ConfirmDeleteResult ShowConfirmDelete(ConfirmDeleteRequest request);
}

/// <summary>Beschreibung der zu löschenden Entität + ihrer FK-Abhängigkeiten.</summary>
public sealed record ConfirmDeleteRequest(
    string EntityName,          // "Buch", "Autor", "Verlag", "Ort"
    string EntityLabel,         // "Das Parfum (1985)" — Anzeige in der Bestätigung
    IReadOnlyList<string> AffectedEntries, // ["Buch: Narziss und Goldmund", ...]
    bool CanCascade);           // true, wenn OnDelete=SetNull für mind. eine FK ist

/// <summary>Ergebnis des Confirm-Delete-Dialogs.</summary>
public sealed record ConfirmDeleteResult(
    bool Confirmed,
    bool Cascade);
