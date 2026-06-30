using System.Windows;
using BibWpf.ViewModels;

namespace BibWpf;

/// <summary>
/// Hauptfenster. Sein DataContext wird via DI in <see cref="App.OnStartup"/>
/// gesetzt, NICHT im Konstruktor.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
