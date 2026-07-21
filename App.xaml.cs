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
    private IServiceScope? _appScope;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Globale Fehlerbehandlung registrieren
        AppDomain.CurrentDomain.UnhandledException += (s, ev) =>
        {
            var exception = ev.ExceptionObject as Exception;
            MessageBox.Show($"Unbehandelter Systemfehler:\n\n{exception?.ToString()}", "Schwerwiegender Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (s, ev) =>
        {
            MessageBox.Show($"Unbehandelter UI-Fehler:\n\n{ev.Exception?.ToString()}", "UI-Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            ev.Handled = true;
        };

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

                // ---- Services ----
                services.AddSingleton<BibWpf.ViewModels.Dialogs.IDialogService, BibWpf.Services.WpfDialogService>();
                services.AddScoped<IEditPanelService, EditPanelService>();

                // ---- ViewModels ----
                // Sub-ViewModels: scoped, damit sie den (scoped) DbContext teilen.
                services.AddScoped<BuecherViewModel>();
                services.AddScoped<AutorenViewModel>();
                services.AddScoped<VerlageViewModel>();
                services.AddScoped<OrteViewModel>();
                services.AddScoped<SettingsViewModel>();

                // Edit-Dialog ViewModels (erzeugt via ActivatorUtilities im Service).
                services.AddTransient<BibWpf.ViewModels.Dialogs.BuecherEditDialogViewModel>();
                services.AddTransient<BibWpf.ViewModels.Dialogs.AutorenEditDialogViewModel>();
                services.AddTransient<BibWpf.ViewModels.Dialogs.VerlageEditDialogViewModel>();
                services.AddTransient<BibWpf.ViewModels.Dialogs.OrteEditDialogViewModel>();

                // MainViewModel: scoped (hält die injizierten Sub-VMs über die Session).
                services.AddScoped<MainViewModel>();

                // MainWindow: scoped, damit es denselben Scope wie MainViewModel verwendet.
                services.AddScoped<MainWindow>();
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
            db.Database.Migrate();
            if (db.IsDatabaseEmpty())
            {
                // Automatisch mit Testdaten befüllen, damit die App ohne Dialog startet.
                db.EnsureSeeded();
            }
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

        // ---- MainWindow in einem eigenen Scope auflösen (Scoped-VMs teilen diesen Scope) ----
        _appScope = _host.Services.CreateScope();
        var mainWindow = _appScope.ServiceProvider.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _appScope?.Dispose();
        if (_host is not null)
        {
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
