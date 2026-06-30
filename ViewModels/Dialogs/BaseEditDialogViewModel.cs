using System.ComponentModel.DataAnnotations;
using BibWpf.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace BibWpf.ViewModels.Dialogs;

/// <summary>
/// Ergebniswert eines mod­alen Edit-Dialogs.
/// </summary>
public enum EditDialogResult
{
    /// <summary>Dialog wurde mit "Abbrechen" geschlossen — keine Änderung.</summary>
    Cancel = 0,

    /// <summary>Dialog wurde gespeichert — Entität wurde erfolgreich persistiert.</summary>
    Saved = 1,

    /// <summary>Dialog wurde mit "Abbrechen" geschlossen — ein Delete-Request wurde ausgelöst.</summary>
    Deleted = 2,
}

/// <summary>
/// Basisklasse für alle EditDialog-ViewModels. Erbt von <see cref="ObservableValidator"/>,
/// hält die zu bearbeitende Entität und koordiniert Save / Cancel / DB-Constraint-Fehler.
/// </summary>
/// <typeparam name="T">Domain-Entität (z. B. <c>Buch</c>, <c>Autor</c>, <c>Verlag</c>, <c>Ort</c>).</typeparam>
public abstract partial class BaseEditDialogViewModel<T> : ObservableValidator
    where T : class, new()
{
    private readonly LibraryDbContext _db;

    [ObservableProperty]
    private T _entity = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDbErrors))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string? _dbErrorMessage;

    /// <summary>Wird vom Code-Behind des Dialogs abonniert, um das Fenster zu schließen.</summary>
    public event EventHandler<EditDialogResultEventArgs>? RequestClose;

    public bool HasDbErrors => !string.IsNullOrEmpty(DbErrorMessage);

    /// <summary>Überschreiben: Window-Titel.</summary>
    public abstract string Title { get; }

    /// <summary>Wird nach dem Konstruktor vom abgeleiteten VM aufgerufen, um Validierung zu triggern.</summary>
    public void ValidateAll()
        => ValidateAllProperties();

    protected BaseEditDialogViewModel(LibraryDbContext db, T? existing)
    {
        _db = db;
        if (existing is not null)
            Entity = existing;

        // Validierung initial anstoßen, damit HasErrors direkt korrekt ist.
        ValidateAll();
    }

    /// <summary>
    /// Persistiert die Entität. Wird vom SaveCommand aufgerufen. Bei Erfolg wird
    /// <see cref="RequestClose"/> mit <see cref="EditDialogResult.Saved"/> gefeuert.
    /// Bei DB-Constraint-Verletzungen wird <see cref="DbErrorMessage"/> gesetzt.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (Entity is null) return;

        // Neu hinzufügen oder bestehendes updaten — Change-Tracker entscheidet das
        // anhand der vom EF-Context beim Tracking erkannten Entität.
        if (_db.Entry(Entity).State == EntityState.Detached)
            _db.Add(Entity);

        try
        {
            await _db.SaveChangesAsync();
            RequestClose?.Invoke(this, new EditDialogResultEventArgs(EditDialogResult.Saved));
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            DbErrorMessage = "Ein Datensatz mit diesen Werten existiert bereits (Eindeutigkeits-Constraint).";
        }
        catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
        {
            DbErrorMessage = "Referenzierter Datensatz (z. B. Autor/Verlag/Ort) existiert nicht mehr.";
        }
        catch (DbUpdateException ex)
        {
            DbErrorMessage = $"Datenbankfehler: {ex.InnerException?.Message ?? ex.Message}";
        }
    }

    private bool CanSave() => !HasErrors;

    partial void OnEntityChanged(T value)
    {
        // Wenn die Entität ausgetauscht wird (Add vs. Edit), Validierung neu anstoßen
        // und SaveCommand-Zustand invalidieren.
        ValidateAll();
        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void Cancel()
        => RequestClose?.Invoke(this, new EditDialogResultEventArgs(EditDialogResult.Cancel));

    // -------- Postgres-Constraint-Detection (23505 unique, 23503 FK) --------

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is Npgsql.PostgresException pg && pg.SqlState == "23505";

    private static bool IsForeignKeyViolation(DbUpdateException ex)
        => ex.InnerException is Npgsql.PostgresException pg && pg.SqlState == "23503";
}

/// <summary>EventArgs, die vom VM an das Code-Behind des Dialogs gehen.</summary>
public sealed class EditDialogResultEventArgs : EventArgs
{
    public EditDialogResult Result { get; }
    public EditDialogResultEventArgs(EditDialogResult result) => Result = result;
}
