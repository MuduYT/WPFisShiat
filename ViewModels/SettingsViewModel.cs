using System.Windows;
using BibWpf.Data;
using CommunityToolkit.Mvvm.Input;

namespace BibWpf.ViewModels;

/// <summary>
/// ViewModel für die Einstellungen (Datenbank zurücksetzen / löschen / neu befüllen).
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly LibraryDbContext _db;

    public SettingsViewModel(LibraryDbContext db)
    {
        _db = db;
        Title = "Einstellungen";
    }

    [RelayCommand]
    private async Task ResetDatabaseAsync()
    {
        var confirmReset = MessageBox.Show(
            "Wollen Sie die Datenbank wirklich zurücksetzen / löschen?\nAlle vorhandenen Daten gehen verloren!",
            "Datenbank zurücksetzen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmReset != MessageBoxResult.Yes)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            // 1. Reset / Truncate durchführen
            await _db.ResetDatabaseAsync();

            // 2. Abfragen, ob neue Testdaten generiert werden sollen
            var confirmSeed = MessageBox.Show(
                "Datenbank erfolgreich geleert.\n\nWollen Sie neue Testdaten generieren?",
                "Testdaten generieren",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmSeed == MessageBoxResult.Yes)
            {
                await _db.EnsureSeededAsync();
                MessageBox.Show(
                    "Datenbank wurde erfolgreich zurückgesetzt und mit standardisierten Testdaten befüllt.",
                    "Erfolg",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    "Datenbank wurde erfolgreich geleert.",
                    "Erfolg",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Zurücksetzen der Datenbank: {ex.Message}";
            MessageBox.Show(
                $"Fehler beim Zurücksetzen:\n{ex.Message}",
                "Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
