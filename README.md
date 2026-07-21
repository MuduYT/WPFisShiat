# BibWpf — Bibliothek-Verwaltung (WPF + EF Core)

## Projektübersicht

WPF-Anwendung zur Verwaltung einer Bibliothek mit Autoren, Büchern, Verlagen und Orten. Verwendet .NET 8, Entity Framework Core mit PostgreSQL, CommunityToolkit.Mvvm und Generic Host mit Dependency Injection.

## Technologie-Stack

- .NET 8.0 (WPF)
- Entity Framework Core 8.0.12 + PostgreSQL (Npgsql)
- CommunityToolkit.Mvvm 8.4.0
- Microsoft.Extensions.Hosting (DI + Konfiguration)

## Ordner-Struktur

```
BibWpf/
├── App.xaml / App.xaml.cs          # Anwendung, Ressourcen, DI-Host, DB-Initialisierung
├── appsettings.json                 # ConnectionString + Logging
├── BibWpf.csproj                    # Projektdatei
├── AssemblyInfo.cs                  # ThemeInfo
│
├── Converters/                      # BoolToVisibility, StringToVisibility, DataGridIndexHelper
├── Data/                            # LibraryDbContext, LibraryDbContextFactory
├── Models/                          # Autor, Buch, Ort, Verlag
├── Services/                        # WpfDialogService (IDialogService-Implementierung)
├── ViewModels/                      # BaseViewModel, Sub-VMs (Buecher, Autoren, Verlage, Orte, Settings)
│   └── Dialogs/                     # BaseEditDialogViewModel, spezifische Edit-VMs
├── Views/                           # BuecherView, AutorenView, VerlageView, OrteView, SettingsView
├── Dialogs/                         # BuecherEditDialog, AutorenEditDialog, VerlageEditDialog, OrteEditDialog, ConfirmDeleteWindow, EditDialogBase
└── Migrations/                      # EF Core Migrations
```

## Datenbank

PostgreSQL. ConnectionString in `appsettings.json`. Migrationen werden beim App-Start automatisch ausgeführt (`db.Database.Migrate()`).

## Funktionalität

- CRUD für Bücher, Autoren, Verlage, Orte
- Modale Edit-Dialoge mit Live-Validierung (DataAnnotations + ObservableValidator)
- Zwei-Stufen-Löschen mit Prüfung abhängiger Einträge
- Dark-Theme-Design mit einheitlicher Farbpalette
- Daten-Grid mit automatischer Index-Spalte und Sortier-Unterstützung

## Fehler / Hinweise

- Keine kritischen Fehler im Code gefunden.
- `.vs`-Ordner bleibt teilweise bestehen (wird von Visual Studio genutzt).
- Nicht mehr verwendete Dateien (`$null`, `bin`, `obj`, `.idea`, `plans`) wurden entfernt.

## Starten

1. PostgreSQL-Server starten (Port 5433, DB `bibliothek`, User `bibuser`)
2. `dotnet build`
3. `dotnet run` oder über Visual Studio starten
