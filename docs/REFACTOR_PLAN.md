# Refactor-Plan: BibWpf auf Review-fähiges Minimum reduzieren

> Ziel: Das Projekt so vereinfachen, dass jeder einzelne Teil beim Review erklärt werden kann — ohne auswendig gelernte Floskeln.
> Zeitplan: Schrittweise ab morgen durchführen. Keine Phase auf einmal, nach jeder Phase `dotnet build` ausführen.

---

## Ausgangslage

Das aktuelle Projekt funktioniert technisch, ist aber überkonstruiert für eine Lern-App:

- Generic Host + DI (Microsoft.Extensions.Hosting)
- Repository-Pattern (`IRepository<T>`, `Repositories.cs`)
- `async/await` überall, auch dort, wo es nicht nötig ist
- Slide-in Edit-Panel mit ViewModel-Wechsel
- Zentrale Ressourcen, Themes, FieldLimits, DataAnnotations-Validierung
- Viele automatisch generierte Migrationsdateien

Das führt dazu, dass das Programm von außen aussieht wie eine einfache Bücherverwaltung, der Code aber wie ein Enterprise-Projekt wirkt.

---

## Refactor-Prinzipien

1. **Weniger ist mehr.** Jede Klasse, jedes Interface und jede Abstraktion muss sich rechtfertigen.
2. **Nach jeder Phase bauen.** `dotnet build` muss erfolgreich sein, bevor die nächste Phase beginnt.
3. **Nur eine Sache auf einmal ändern.** Nie gleichzeitig DI, Repository und Validierung anfassen.
4. **Erklärbarkeit vor Architektur.** Ein simpler Code-Behind-Ansatz, der erklärt werden kann, ist besser als ein perfektes MVVM-Pattern, das niemand versteht.
5. **Backup-Strategie beachten.** Der aktuelle Stand bleibt in Git erhalten. Bei Panne kann jederzeit zurückgesetzt werden.

---

## Phase 0: Sicherer Startpunkt (heute / vor morgen)

- [ ] In `C:\dev\BibWpf` ein sauberes `dotnet build` durchführen und dokumentieren.
- [ ] `git status` prüfen: alle Änderungen committed?
- [ ] Einen neuen Branch `refactor/simplify` erstellen, damit `main` unverändert bleibt.
- [ ] README prüfen: passt es noch zum aktuellen Stand?

**Erfolgskriterium:** `dotnet build` läuft ohne Fehler auf dem neuen Branch.

---

## Phase 1: Auswahl der Features reduzieren

Idee: Nur noch die Entitäten behalten, die wirklich im Review gezeigt werden.

- [ ] Entscheiden: Sollen `Ort` und `Verlag` rausfliegen?
- [ ] Wenn ja: `Ort.cs`, `Verlag.cs`, `OrteViewModel`, `VerlageViewModel`, `OrteEditViewModel`, `VerlageEditViewModel` und zugehörige Views entfernen.
- [ ] `LibraryDbContext` anpassen (nur `Buch` und `Autor` behalten).
- [ ] Migrations neu erstellen oder alte entfernen und eine einzelne Initial-Migration neu generieren.

**Empfehlung:** Nur `Buch` und `Autor` behalten. Das reicht für den Review und ist viel leichter zu erklären.

**Erfolgskriterium:** App startet, Liste von Büchern und Autoren lädt, Build geht.

---

## Phase 2: Repository-Pattern entfernen

Idee: EF Core ist selbst schon genug. Die `IRepository<T>`-Schicht ist reiner Overhead.

- [ ] `IRepository.cs` löschen.
- [ ] `Repositories.cs` löschen.
- [ ] Alle `using var repo = ...` in den ViewModels durch `using var db = new LibraryDbContext()` ersetzen.
- [ ] Methodenaufrufe `repo.GetAllAsync()` durch `db.Buecher.ToList()` ersetzen.

**Erfolgskriterium:** `dotnet build` ohne Fehler, CRUD für verbliebene Entitäten funktioniert.

---

## Phase 3: Generic Host + DI entfernen

Idee: Eine WPF-Desktop-App braucht keinen `IHost` mit `ServiceProvider`.

- [ ] `App.xaml.cs` öffnen und `IHost`/`IServiceProvider` entfernen.
- [ ] `MainWindow` erstellt `MainViewModel` direkt.
- [ ] `MainViewModel` erstellt `LibraryDbContext` direkt oder bekommt ihn per Parameter.
- [ ] Alle Stellen, an denen `IServiceProvider` verwendet wird, anpassen.

**Erfolgskriterium:** App startet ohne `Host`, keine `IServiceProvider`-Aufrufe mehr im Code.

---

## Phase 4: Async/await entfernen

Ideen: Bei einer lokalen Datenbank sind `async`-Methoden unnötige Komplexität.

- [ ] In `MainViewModel` `LoadAsync` durch `Load` ersetzen.
- [ ] In den ViewModels `Load...Async` durch `Load...` ersetzen.
- [ ] Edit-Dialog-Initialisierung synchron machen.
- [ ] Command-Methoden (`Save`, `Delete`) synchron machen.

**Erfolgskriterium:** Keine `async void` mehr außer bei Event-Handlern. Build geht.

---

## Phase 5: Edit-Panel vereinfachen

Idee: Slide-in-Panel mit ViewModel-Wechsel ist schwer nachvollziehbar.

- [ ] `AutorenEditView` und `BuecherEditView` als eigenständige `Window` oder `UserControl` umbauen.
- [ ] `MainViewModel.OpenEditAsync` entfernen.
- [ ] Stattdessen einfache Methode: `ShowEditDialog(item)` öffnet ein neues Fenster und gibt `true`/`false` zurück.
- [ ] `BaseEditDialogViewModel` und `IBaseEditDialogViewModel` vereinfachen oder entfernen.

**Alternative:** Wenn Slide-in unbedingt bleiben soll, dann wenigstens ein statisches UserControl pro Entität statt dynamischem ViewModel-Wechsel.

**Erfolgskriterium:** Editieren funktioniert, Code ist nachvollziehbar.

---

## Phase 6: Validierung vereinfachen

Idee: `[Range]`, `FieldLimits`, `IDataErrorInfo` sind nice-to-have, aber zu viel für den Anfang.

- [ ] `FieldLimits.cs` entfernen.
- [ ] `DataAnnotations` in den Models entfernen.
- [ ] Einfache `if`-Prüfungen vor `db.SaveChanges()` einbauen (z. B. `if (string.IsNullOrWhiteSpace(buch.Titel))`).
- [ ] Bei numerischen Feldern (Seiten, Geburtsjahr, Erscheinungsjahr) prüfen, ob der Wert im sinnvollen Bereich liegt.

**Erfolgskriterium:** Speichern zeigt Fehlermeldung bei ungültigen Daten, Build geht.

---

## Phase 7: Themen/Resourcen optional machen

Ideen: Themes, zentrale Resourcen und Converters sind nicht notwendig.

- [ ] `Themes/Theme.xaml` prüfen: wird wirklich gebraucht?
- [ ] `Properties/Resources.resx` prüfen: können die Strings direkt in XAML stehen?
- [ ] Converters entfernen, wenn sie nicht mehr gebraucht werden.

**Nur machen, wenn Zeit bleibt.** Diese Phase ist optional.

---

## Phase 8: Code bereinigen und Review vorbereiten

- [ ] Überflüssige `using`-Anweisungen entfernen.
- [ ] Leere Klassen/Interfaces löschen.
- [ ] `docs/`-Ordner aufräumen: alte Dokumente archivieren oder löschen.
- [ ] Neuer kurzer `README.md` schreiben, der den aktuellen Stand erklärt.
- [ ] Review-Fragen vorbereiten:
  - Warum hast du EF Core genutzt?
  - Wie funktioniert das Binding zwischen XAML und ViewModel?
  - Was passiert beim Speichern?
  - Warum hast du Repository/Generic Host entfernt?

---

## Risiken und Fallback

| Risiko | Fallback |
|---|---|
| Refactor dauert länger als gedacht | Branch `refactor/simplify` löschen, auf `main` zurückwechseln, original Projekt zeigen |
| Build zerstört | Schrittweise `git checkout -- .` für einzelne Dateien, nicht alles auf einmal |
| Zeit reicht nicht bis zum Review | Nur Phase 0–2 machen, Rest ehrlich beim Review erklären |
| Reviewer fragt nach Architektur | Ehrlich sagen, dass bewusst vereinfacht wurde, um es besser zu verstehen |

---

## Empfohlene Reihenfolge für morgen

1. **Morgens:** Phase 0 (Branch, Build-Check).
2. **Vormittag:** Phase 1 (Features reduzieren) + Phase 2 (Repository entfernen).
3. **Nachmittag:** Phase 3 (Generic Host entfernen) + Phase 4 (Async entfernen).
4. **Später / nächster Tag:** Phase 5 (Edit-Panel) + Phase 6 (Validierung).
5. **Optional:** Phase 7 (Themes) + Phase 8 (Review-Prep).

---

## Wichtigste Erkenntnis

Das Programm von außen muss nicht anders aussehen. Eine einfachere Innenarchitektur ist genauso gut — und für den Review deutlich besser.
