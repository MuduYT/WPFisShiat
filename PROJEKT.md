# Projektdokumentation — BibWpf

## Zweck
Strukturierung und Bereinigung des Projekts. Keine funktionalen Änderungen an der Anwendung.

## Durchgeführte Änderungen

1. Unnötige Dateien gelöscht:
   - `$null` (leere Datei, 94 Byte)
   - `bin/` und `obj/` (Build-Artefakte)
   - `.idea/` (JetBrains IDE-Dateien)
   - `plans/` (Plan-Dokumentation, nicht projekt-relevant)
   - `.vs/` teilweise bereinigt (Visual Studio temporäre Dateien, einige durch Prozess-Sperre nicht löschbar)

2. Struktur bestätigt:
   - Alle Quellordner (`Converters`, `Data`, `Dialogs`, `Migrations`, `Models`, `Services`, `ViewModels`, `Views`) bleiben erhalten.
   - Keine Dateien verschoben oder umbenannt.

3. Fehleranalyse:
   - Keine kritischen Fehler in `App.xaml`, `App.xaml.cs`, `BibWpf.csproj`, `LibraryDbContext.cs`, `BaseEditDialogViewModel.cs`, `WpfDialogService.cs` oder den Views gefunden.
   - Die Anwendung verwendet korrekt DI (`IHost`), EF Core Migrations und modale Dialoge.

4. Dokumentation erstellt:
   - `README.md` mit Übersicht, Technologie-Stack, Ordner-Struktur, Datenbank-Hinweis, Funktionalität und Fehlerhinweis.

## Verbleibende Hinweise

- `.vs/` enthält noch leere Verzeichnisse, die von Visual Studio neu erstellt werden.
- Das Projekt kann mit `dotnet build` und `dotnet run` gestartet werden (PostgreSQL erforderlich).
