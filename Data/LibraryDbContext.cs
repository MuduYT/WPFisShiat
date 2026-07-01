using BibWpf.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BibWpf.Data;

/// <summary>
/// EF-Core-DbContext für die Bibliothek.
///
/// Seed-/Reset-Logik (Spezial-Anforderung):
///   - <see cref="EnsureSeededAsync"/>: Wird beim App-Start aufgerufen. Erkennt, ob die
///     Datenbank *komplett leer* ist, und generiert in diesem Fall je 3 realistische
///     Testdaten pro Kategorie (Orte, Verlage, Autoren, Bücher).
///   - <see cref="ResetDatabaseAsync"/>: Leert *alle* Tabellen (TRUNCATE … RESTART
///     IDENTITY CASCADE). Die DB bleibt danach leer für eigene Daten. Erst beim nächsten
///     App-Start greift wieder die leere-DB-Erkennung und erzeugt die Testdaten neu.
/// </summary>
public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    // ---- DbSets (Tabellen) ----
    public DbSet<Ort> Orte => Set<Ort>();
    public DbSet<Verlag> Verlage => Set<Verlag>();
    public DbSet<Autor> Autoren => Set<Autor>();
    public DbSet<Buch> Buecher => Set<Buch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tabellennamen explizit festlegen, damit ResetDatabase-TRUNCATE exakt passt.
        modelBuilder.Entity<Ort>(b =>
        {
            b.ToTable("Orte");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(150);
            b.Property(x => x.Land).HasMaxLength(100);
            b.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Verlag>(b =>
        {
            b.ToTable("Verlage");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Gruendungsjahr);

            // Verlag -> Ort (optional)
            b.HasOne(x => x.Ort)
             .WithMany(o => o.Verlage)
             .HasForeignKey(x => x.OrtId)
             .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Autor>(b =>
        {
            b.ToTable("Autoren");
            b.HasKey(x => x.Id);
            b.Property(x => x.Vorname).IsRequired().HasMaxLength(100);
            b.Property(x => x.Nachname).IsRequired().HasMaxLength(100);
            b.Property(x => x.Geburtsjahr);
            b.Ignore(x => x.VollstaendigerName); // computed, nicht gemappt

            // Autor -> Ort (optional)
            b.HasOne(x => x.Ort)
             .WithMany(o => o.Autoren)
             .HasForeignKey(x => x.OrtId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Buch>(b =>
        {
            b.ToTable("Buecher");
            b.HasKey(x => x.Id);
            b.Property(x => x.Titel).IsRequired().HasMaxLength(300);
            b.Property(x => x.Isbn).HasMaxLength(20);
            b.Property(x => x.Erscheinungsjahr).IsRequired();
            b.Property(x => x.Seiten);

            // Buch -> Autor (Pflicht)
            b.HasOne(x => x.Autor)
             .WithMany(a => a.Buecher)
             .HasForeignKey(x => x.AutorId)
             .OnDelete(DeleteBehavior.Restrict);

            // Buch -> Verlag (Pflicht)
            b.HasOne(x => x.Verlag)
             .WithMany(v => v.Buecher)
             .HasForeignKey(x => x.VerlagId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.Titel);
        });
    }

    // ====================================================================
    //  Seed-Logik: erkennt "komplett leere DB" und befüllt mit Testdaten
    // ====================================================================

    /// <summary>
    /// Prüft beim Start, ob die Datenbank komplett leer ist, und erzeugt ggf. die
    /// Testdaten (je 3 pro Kategorie). Ist in beiden Fällen idempotent.
    /// </summary>
    public async Task EnsureSeededAsync(CancellationToken ct = default)
    {
        // "Komplett leer" = in keiner der vier Tabellen existiert ein Datensatz.
        bool isEmpty = !await Orte.AnyAsync(ct)
                    && !await Verlage.AnyAsync(ct)
                    && !await Autoren.AnyAsync(ct)
                    && !await Buecher.AnyAsync(ct);

        if (!isEmpty)
            return; // bereits Daten vorhanden → nicht anfassen

        await using IDbContextTransaction tx = await Database.BeginTransactionAsync(ct);
        try
        {
            BuildSeedData(
                out List<Ort> orte,
                out List<Verlag> verlage,
                out List<Autor> autoren,
                out List<Buch> buecher);

            // Reihenfolge wichtig: erst Eltern (Orte), dann Verlage/Autoren, dann Bücher.
            // Der ChangeTracker löst die Fremdschlüssel über die Navigationen automatisch auf.
            await Orte.AddRangeAsync(orte, ct);
            await Verlage.AddRangeAsync(verlage, ct);
            await Autoren.AddRangeAsync(autoren, ct);
            await Buecher.AddRangeAsync(buecher, ct);

            await SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>Synchrone Variante (z. B. für Design-Time/Tests).</summary>
    public void EnsureSeeded()
    {
        bool isEmpty = !Orte.Any()
                    && !Verlage.Any()
                    && !Autoren.Any()
                    && !Buecher.Any();
        if (!isEmpty) return;

        using IDbContextTransaction tx = Database.BeginTransaction();
        try
        {
            BuildSeedData(out var orte, out var verlage, out var autoren, out var buecher);
            Orte.AddRange(orte);
            Verlage.AddRange(verlage);
            Autoren.AddRange(autoren);
            Buecher.AddRange(buecher);
            SaveChanges();
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Erzeugt die Testdaten (3 je Kategorie) als verknüpften Objekt-Graph.
    /// Navigationen werden gesetzt, damit der ChangeTracker die Fremdschlüssel ableitet.
    /// </summary>
    /// <summary>
    /// Prüft, ob die Datenbank komplett leer ist.
    /// </summary>
    public bool IsDatabaseEmpty()
    {
        return !Orte.Any()
            && !Verlage.Any()
            && !Autoren.Any()
            && !Buecher.Any();
    }

    /// <summary>
    /// Prüft asynchron, ob die Datenbank komplett leer ist.
    /// </summary>
    public async Task<bool> IsDatabaseEmptyAsync(CancellationToken ct = default)
    {
        return !await Orte.AnyAsync(ct)
            && !await Verlage.AnyAsync(ct)
            && !await Autoren.AnyAsync(ct)
            && !await Buecher.AnyAsync(ct);
    }

    private static void BuildSeedData(
        out List<Ort> orte,
        out List<Verlag> verlage,
        out List<Autor> autoren,
        out List<Buch> buecher)
    {
        // --- Orte ---
        var graz = new Ort { Name = "Graz", Land = "Österreich" };
        var klagenfurt = new Ort { Name = "Klagenfurt", Land = "Österreich" };
        var villach = new Ort { Name = "Villach", Land = "Österreich" };
        orte = new List<Ort> { graz, klagenfurt, villach };

        // --- Autoren ---
        var pinzer = new Autor { Vorname = "Nico", Nachname = "Pinzer", Geburtsjahr = 1998, Ort = graz };
        var pucher = new Autor { Vorname = "Fabian", Nachname = "Pucher", Geburtsjahr = 1995, Ort = klagenfurt };
        var hraschan = new Autor { Vorname = "Roland", Nachname = "Hraschan", Geburtsjahr = 1992, Ort = villach };
        autoren = new List<Autor> { pinzer, pucher, hraschan };

        // --- Verlage ---
        var hilfMir = new Verlag { Name = "Hilf Mir! Jung, Pleite, Verzweifelt", Gruendungsjahr = 2020, Ort = graz };
        var gutePucher = new Verlag { Name = "Gute Pucher", Gruendungsjahr = 2018, Ort = klagenfurt };
        var schlechtePucher = new Verlag { Name = "Schlechte Pucher", Gruendungsjahr = 2019, Ort = villach };
        verlage = new List<Verlag> { hilfMir, gutePucher, schlechtePucher };

        // --- Bücher ---
        buecher = new List<Buch>
        {
            new Buch
            {
                Titel = "Wie TikTok mein leben einnahm",
                Isbn = "978-3-123-45678-9",
                Erscheinungsjahr = 2023,
                Seiten = 180,
                Autor = pinzer,
                Verlag = hilfMir
            },
            new Buch
            {
                Titel = "DnB Leichtgemacht",
                Isbn = "978-3-987-65432-1",
                Erscheinungsjahr = 2021,
                Seiten = 120,
                Autor = pucher,
                Verlag = gutePucher
            },
            new Buch
            {
                Titel = "in 1000 Tagen zum Sigma Male",
                Isbn = "978-3-555-66677-7",
                Erscheinungsjahr = 2024,
                Seiten = 300,
                Autor = hraschan,
                Verlag = schlechtePucher
            }
        };
    }

    // ====================================================================
    //  Reset-Logik: leert alle Tabellen (TRUNCATE), ohne neu zu seeden.
    //  Erst beim nächsten App-Start greift EnsureSeededAsync wieder.
    // ====================================================================

    /// <summary>
    /// Leert *alle* Tabellen vollständig (TRUNCATE … RESTART IDENTITY CASCADE).
    /// Danach bleibt die DB leer – ideal, um eigene Daten zu erfassen.
    /// Beim nächsten App-Start wird über <see cref="EnsureSeededAsync"/> neu geseedt.
    /// </summary>
    /// <remarks>
    /// Der ChangeTracker wird vorher gelöscht, damit keine veralteten Entitäten
    /// im Speicher hängen bleiben.
    /// </remarks>
    public async Task ResetDatabaseAsync(CancellationToken ct = default)
    {
        ChangeTracker.Clear();

        // Blatt-Tabelle (Buecher) zuerst, danach Eltern – CASCADE deckt es ohnehin ab.
        // RESTART IDENTITY setzt die Sequenzen zurück, sodass neu geseedte Daten
        // wieder bei Id = 1 beginnen.
        const string sql = """
            TRUNCATE TABLE
                "Buecher",
                "Autoren",
                "Verlage",
                "Orte"
            RESTART IDENTITY CASCADE;
            """;

        await Database.ExecuteSqlRawAsync(sql, ct);
    }

    /// <summary>Synchrone Variante von <see cref="ResetDatabaseAsync"/>.</summary>
    public void ResetDatabase()
    {
        ChangeTracker.Clear();
        const string sql = """
            TRUNCATE TABLE
                "Buecher",
                "Autoren",
                "Verlage",
                "Orte"
            RESTART IDENTITY CASCADE;
            """;
        Database.ExecuteSqlRaw(sql);
    }
}
