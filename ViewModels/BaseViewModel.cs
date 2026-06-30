using CommunityToolkit.Mvvm.ComponentModel;

namespace BibWpf.ViewModels;

/// <summary>
/// Gemeinsame Basis für alle ViewModels der App.
/// Stellt einen einheitlichen Titel und optionale Status-/Busy-Flags
/// bereit, die in der Sidebar/Header angezeigt werden können.
/// </summary>
/// <remarks>
/// Erbt von <see cref="ObservableObject"/> aus CommunityToolkit.Mvvm.
/// Konkrete Sub-ViewModels (Bücher, Autoren, Verlage, Orte) erben hiervon.
/// </remarks>
public abstract partial class BaseViewModel : ObservableObject
{
    /// <summary>Anzeige-Titel für die Sidebar/Header.</summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>Optionales Busy-Flag (für Schritt 3 vorbereitet).</summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>Optionale Fehlermeldung (für Schritt 3 vorbereitet).</summary>
    [ObservableProperty]
    private string? _errorMessage;
}