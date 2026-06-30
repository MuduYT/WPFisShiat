using System.Windows;
using BibWpf.Data;
using BibWpf.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BibWpf;

/// <summary>
/// WPF-App-Bootstrap mit Generic Host + DI.
/// - Baut den Host, registriert DbContext (scoped) + alle ViewModels.
/// - Führt EF-Migration + Seed beim Start aus.
/// - Erzeugt das MainWindow mit dem per DI aufgelösten MainViewModel als DataContext.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(cfg =>
            {
                cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((ctx, services) =>
            {
                // ---- EF Core / DbContext ----
                var connectionString = ctx.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' fehlt in appsettings.json.");

                services.AddDbContext<LibraryDbContext>(opt =>
                    opt.UseNpgsql(connectionString));

                // ---- ViewModels ----
                // Sub-ViewModels: scoped, damit sie den (scoped) DbContext teilen.
                services.AddScoped<BuecherViewModel>();
                services.AddScoped<AutorenViewModel>();
                services.AddScoped<VerlageViewModel>();
                services.AddScoped<OrteViewModel>();

                // MainViewModel: scoped (hält die injizierten Sub-VMs über die Session).
                services.AddScoped<MainViewModel>();

                // MainWindow: transient, bekommt MainViewModel per Konstruktor.
                services.AddTransient<MainWindow>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddDebug();
            })
            .Build();

        // ---- DB-Migration + Seed ----
        try
        {
            using var scope = _host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            await db.Database.MigrateAsync();
            await db.EnsureSeededAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Datenbank-Initialisierung fehlgeschlagen:\n\n{ex.Message}",
                "Bibliothek — Startfehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        // ---- MainWindow aus dem DI-Container holen ----
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
