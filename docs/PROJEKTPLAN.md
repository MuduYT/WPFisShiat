# PROJEKTPLAN — Finalisierung BibWpf → LibraryApp

> **Stand:** 2026-07-21 · **Status:** Wartet auf Freigabe
> **Projekt:** WPF-Bibliotheksverwaltung (.NET 8, EF Core 8 + PostgreSQL, CommunityToolkit.Mvvm)
> **Ziel:** Code-Review-reife Abgabe (überbetriebliche Lehrausbildung)

---

## 1. Ist-Analyse (Kurzfassung)

| Bereich | Befund |
|---|---|
| Struktur | Einzelprojekt `BibWpf` im Repo-Root; kein `/src`, kein `/docs`, keine Tests. `bin/` teilweise git-getrackt. |
| Architektur | DI via Generic Host ✔ · MVVM via CommunityToolkit ✔ · **DbContext direkt in ViewModels** ✘ (kein Repository) |
| Toter Code | Modale Edit-Dialoge (`Dialogs/*EditDialog*`, `EditDialogBase`, `WpfDialogService.ShowEdit*`) werden nicht mehr genutzt — Bearbeiten läuft über das Slide-In-Panel. Nur `ConfirmDeleteWindow` ist noch aktiv. |
| Commands | `[RelayCommand]` mit `CanExecute` + `NotifyCanExecuteChangedFor` ✔ · Ein `async void` (Event-Handler `OnEditRequestClose`, zulässig) · Reflection-Aufruf von `ReloadAsync` in `MainViewModel` ✘ |
| Validierung | DataAnnotations + `ObservableValidator` ✔ · **Widersprüche zu EF-MaxLengths** (Titel 200/300, Verlag 150/200, Ort 120/150 u. a.) ✘ |
| UI | Zentrale Farb-/Stil-Definitionen in `App.xaml` ✔ (aber nicht als eigenes ResourceDictionary) · Alle Texte hardcoded, kein `.resx` ✘ |
| Build | Kein `TreatWarningsAsErrors` ✘ |
| Sprache | Bezeichner durchgehend **Deutsch** (Buch, Autor, Buecher, Titel …) ✘ lt. Vorgabe Englisch |
| Doku | `///`-Doku gut vorhanden (deutsch ✔), aber lückenhaft; doppelte `<summary>` in `LibraryDbContext`; veralteter Kommentar in `Buch` ("Erscheinungsort" existiert nicht mehr) |

---

## 2. Entscheidungspunkte (bitte bei Freigabe beantworten)

**E1 — Englische Bezeichner vs. DB-Schema (betrifft D1):**
Umbenennung `Buch→Book`, `Autor→Author` usw. würde standardmäßig Tabellen-/Spaltennamen ändern → Migration nötig (laut Vorgabe nicht ohne Freigabe).
**Empfehlung:** Klassen/Properties auf Englisch umbenennen, DB-Namen per explizitem Mapping (`ToTable`/`HasColumnName`) beibehalten → **keine Migration, kein Datenverlust**.
Alternativen: (a) volle Umbenennung inkl. Rename-Migration, (b) deutsche Bezeichner behalten (Abweichung von der Vorgabe dokumentieren).

**E2 — Projektname:** `BibWpf` → `LibraryApp` (Ordner `/src/LibraryApp`, Namespace, Assembly). Empfehlung: ja, konsequent umbenennen.

**E3 — Tests:** xUnit-Tests sind sinnvoll für Validierung + Repository (mit SQLite-InMemory-Provider; die Npgsql-spezifische Reset-Logik wird gemockt/ausgenommen). Empfehlung: schlankes Testprojekt (Nice-to-have, siehe F1).

---

## 3. Aufgabenplan

Aufwand: 1 Pomodoro (🍅) = 25 min. Priorität: **M**ust / **S**hould / **N**ice-to-have.

### Block A — Struktur & Build

| # | Aufgabe | Prio | Aufwand | Abhängig von |
|---|---|---|---|---|
| A1 | Ordnerstruktur: Projekt nach `/src/LibraryApp` verschieben, `/docs` + ggf. `/tests` anlegen, `.gitignore` (bin/obj/.vs), git-Tracking bereinigen, Solution-Datei anpassen | M | 2 🍅 | — |
| A2 | Namespaces + Projektname `BibWpf` → `LibraryApp` überall anpassen (inkl. XAML `clr-namespace`) | M | 2 🍅 | A1, E2 |
| A3 | Toten Code entfernen: `Dialogs/*EditDialog*`, `EditDialogBase`, `IDialogService.ShowEdit*`, `WpfDialogService.ShowEdit*`, ungenutzte Sync-Methode `ResetDatabase()`; doppelte XML-Summaries fixen | M | 1 🍅 | — |
| A4 | `TreatWarningsAsErrors` (Release) aktivieren, alle Warnings beheben | M | 1–2 🍅 | A1–A3, B, C, D (zum Schluss) |

### Block B — Architektur & Akzeptanzkriterien

| # | Aufgabe | Prio | Aufwand | Abhängig von |
|---|---|---|---|---|
| B1 | Repository-Schicht: `IRepository<T>`/spezifische Interfaces in `Services/`, ViewModels nutzen nur noch Interfaces (kein DbContext in VMs/Views) | M | 3–4 🍅 | A1/A2 |
| B2 | `MainViewModel`: Reflection-`ReloadAsync` durch Interface (`IReloadable`) ersetzen; `async void`-Handler absichern (try/catch) | S | 1 🍅 | B1 |
| B3 | Validierung konsistent: DataAnnotations ↔ EF-MaxLengths abgleichen (eine Quelle der Wahrheit, Konstanten) | M | 1 🍅 | — |
| B4 | `SettingsViewModel`: `MessageBox`-Aufrufe hinter `IDialogService` kapseln | S | 1 🍅 | B1 |

### Block C — UI-Konsistenz

| # | Aufgabe | Prio | Aufwand | Abhängig von |
|---|---|---|---|---|
| C1 | Styles aus `App.xaml` in eigenes ResourceDictionary `Themes/Theme.xaml` auslagern (Farben/Schriften/Abstände); Spacing-Raster 8/16/24 als Ressourcen definieren | M | 2 🍅 | A1/A2 |
| C2 | Alle UI-Texte nach `Resources.resx` verschieben (XAML via `x:Static`, C#-Fehlermeldungen aus Resources) | M | 3 🍅 | A2 |
| C3 | Margin/Padding-Durchsicht aller Views auf das 8/16/24-Raster | S | 1 🍅 | C1 |

### Block D — Bezeichner Englisch

| # | Aufgabe | Prio | Aufwand | Abhängig von |
|---|---|---|---|---|
| D1 | Modelle + Properties auf Englisch (`Buch→Book`, `Titel→Title` …), DB-Namen via explizitem Mapping stabil halten (**keine Migration**, siehe E1) | M | 3 🍅 | E1-Freigabe, A2 |
| D2 | ViewModels/Views/Services-Bezeichner auf Englisch (`BuecherViewModel→BooksViewModel` …); UI-Texte bleiben deutsch (via resx) | M | 2 🍅 | D1 |

### Block E — Kommentare & Doku

| # | Aufgabe | Prio | Aufwand | Abhängig von |
|---|---|---|---|---|
| E1 | XML-Doku (`///`) vervollständigen: jede öffentliche Klasse/Methode, `<example>` wo sinnvoll; Dateiheader (Zweck/Autor/Datum); veraltete Kommentare korrigieren; auskommentierte Reste entfernen | M | 2–3 🍅 | D2 (nach Renames) |
| E2 | `docs/ARCHITEKTUR.md`: Layer-Diagramm, Datenfluss (View→VM→Repository→DbContext→PostgreSQL), DB-Schema | M | 2 🍅 | B1 |
| E3 | `docs/REVIEW_CHECKLISTE.md`: prüfbare Punkte je Akzeptanzkriterium | M | 1 🍅 | — |
| E4 | `docs/ZEITPROTOKOLL.md` (Tabelle) + `docs/CHANGELOG.md` (alle Änderungen dieser Finalisierung) | M | 1 🍅 | laufend |

### Block F — Qualität & Abnahme

| # | Aufgabe | Prio | Aufwand | Abhängig von |
|---|---|---|---|---|
| F1 | `tests/LibraryApp.Tests` (xUnit): Validierungs- und Repository-Tests (SQLite in-memory) | N | 3 🍅 | B1, E3-Freigabe |
| F2 | Startzeit messen: MainWindow < 1 s kalt (Hinweis: `Database.Migrate()` läuft synchron beim Start — ggf. optimieren/dokumentieren) | S | 1 🍅 | — |
| F3 | Abschluss-Durchlauf: Release-Build ohne Warnings, alle Akzeptanzkriterien einzeln abhaken, App manuell durchklicken (CRUD alle 4 Entitäten) | M | 1–2 🍅 | alles |

---

## 4. Gesamtaufwand

| Priorität | Aufwand |
|---|---|
| Must | ~ 20–24 🍅 ≈ 8–10 h |
| Should | ~ 4 🍅 ≈ 1,5 h |
| Nice-to-have | ~ 3 🍅 ≈ 1,25 h |
| **Gesamt** | **~ 27–31 🍅 ≈ 11–13 h** |

## 5. Empfohlene Reihenfolge

```
A1 → A2 → A3  (Struktur zuerst, sonst doppelte Arbeit bei Renames)
   → D1 → D2  (Renames vor Doku/Resx, sonst alles zweimal anfassen)
   → B1 → B2/B3/B4
   → C1 → C2 → C3
   → E1 → E2/E3/E4
   → F2 → A4 → F3   (Warnings-Gate + Abnahme ganz am Schluss)
F1 optional parallel nach B1.
```

## 6. Status-Tabelle

| # | Aufgabe | Status |
|---|---|---|
| A1 | Ordnerstruktur /src, /docs, .gitignore | offen |
| A2 | Namespace/Projektname LibraryApp | offen |
| A3 | Toten Code entfernen | offen |
| A4 | TreatWarningsAsErrors + Warnings | offen |
| B1 | Repository-Interface | offen |
| B2 | Reflection → IReloadable | offen |
| B3 | Validierung ↔ EF-Mapping abgleichen | offen |
| B4 | MessageBox → IDialogService | offen |
| C1 | Theme.xaml ResourceDictionary | offen |
| C2 | Resources.resx | offen |
| C3 | Spacing-Raster-Durchsicht | offen |
| D1 | Modelle englisch (ohne Migration) | offen |
| D2 | ViewModels/Views englisch | offen |
| E1 | XML-Doku + Dateiheader | offen |
| E2 | ARCHITEKTUR.md | offen |
| E3 | REVIEW_CHECKLISTE.md | offen |
| E4 | ZEITPROTOKOLL.md + CHANGELOG.md | offen |
| F1 | xUnit-Tests | offen |
| F2 | Startzeit < 1 s | offen |
| F3 | Abschluss-Abnahme | offen |

---

## 7. Nicht angefasst (lt. Vorgabe)

- Ausleih-/Rückgabeprozess: **existiert im aktuellen Code nicht** — es wird ohne Freigabe auch keiner hinzugefügt.
- DB-Schema-Migrationen: keine geplant; D1 ist bewusst so ausgelegt, dass das Schema unverändert bleibt.
