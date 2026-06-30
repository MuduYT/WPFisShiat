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
            b.Property(x => x.Beschreibung).HasMaxLength(2000);

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

            // Buch -> Ort (optional)
            b.HasOne(x => x.Ort)
             .WithMany(o => o.Buecher)
             .HasForeignKey(x => x.OrtId)
             .OnDelete(DeleteBehavior.SetNull);

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
    private static void BuildSeedData(
        out List<Ort> orte,
        out List<Verlag> verlage,
        out List<Autor> autoren,
        out List<Buch> buecher)
    {
        // --- Orte ---
        var muenchen = new Ort { Name = "München", Land = "Deutschland" };
        var stuttgart = new Ort { Name = "Stuttgart", Land = "Deutschland" };
        var zuerich = new Ort { Name = "Zürich", Land = "Schweiz" };
        orte = new List<Ort> { muenchen, stuttgart, zuerich };

        // --- Verlage ---
        var diogenes = new Verlag { Name = "Diogenes Verlag", Gruendungsjahr = 1952, Ort = zuerich };
        var klettCotta = new Verlag { Name = "Klett-Cotta", Gruendungsjahr = 1959, Ort = stuttgart };
        var suhrkamp = new Verlag { Name = "Suhrkamp Verlag", Gruendungsjahr = 1950, Ort = null };
        verlage = new List<Verlag> { diogenes, klettCotta, suhrkamp };

        // --- Autoren ---
        var sueskind = new Autor { Vorname = "Patrick", Nachname = "Süskind", Geburtsjahr = 1949, Ort = muenchen };
        var hesse = new Autor { Vorname = "Hermann", Nachname = "Hesse", Geburtsjahr = 1877, Ort = stuttgart };
        var frisch = new Autor { Vorname = "Max", Nachname = "Frisch", Geburtsjahr = 1911, Ort = zuerich };
        autoren = new List<Autor> { sueskind, hesse, frisch };

        // --- Bücher ---
        buecher = new List<Buch>
        {
            new Buch
            {
                Titel = "Das Parfum",
                Isbn = "978-3-257-22800-1",
                Erscheinungsjahr = 1985,
                Seiten = 320,
                Beschreibung = "Die Geschichte des Jean-Baptiste Grenouille im 18. Jahrhundert.",
                Autor = sueskind,
                Verlag = diogenes,
                Ort = zuerich
            },
            new Buch
            {
                Titel = "Narziss und Goldmund",
                Isbn = "978-3-608-93501-6",
                Erscheinungsjahr = 1930,
                Seiten = 416,
                Beschreibung = "Zwei Lebensentwürfe zwischen Geist und Sinnlichkeit.",
                Autor = hesse,
                Verlag = klettCotta,
                Ort = stuttgart
            },
            new Buch
            {
                Titel = "Homo Faber",
                Isbn = "978-3-518-37120-1",
                Erscheinungsjahr = 1957,
                Seiten = 224,
                Beschreibung = "Ein Ingenieur wird durch Zufälle mit seinem Schicksal konfrontiert.",
                Autor = frisch,
                Verlag = suhrkamp,
                Ort = zuerich
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
