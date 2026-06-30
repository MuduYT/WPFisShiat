using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BibWpf.Data;

/// <summary>
/// Design-Time-Factory für EF-Core-Tools.
/// Wird von `dotnet ef migrations add/...` automatisch entdeckt, damit der
/// DbContext auch ohne Dependency-Injection-Container instanziiert werden kann.
/// Die Verbindungszeichenfolge wird aus appsettings.json gelesen.
/// </summary>
public class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
{
    public LibraryDbContext CreateDbContext(string[] args)
    {
        // appsettings.json liegt im Projekt-/Ausgabeverzeichnis
        string baseDir = AppContext.BaseDirectory;
        var config = new ConfigurationBuilder()
            .SetBasePath(baseDir)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

        return new LibraryDbContext(optionsBuilder.Options);
    }
}
