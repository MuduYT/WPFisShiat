using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibWpf.Models;

/// <summary>
/// Eine:n Autor:in eines Buches.
/// </summary>
public class Autor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vorname ist erforderlich.")]
    [StringLength(100, ErrorMessage = "Vorname darf max. 100 Zeichen lang sein.")]
    public string Vorname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nachname ist erforderlich.")]
    [StringLength(100, ErrorMessage = "Nachname darf max. 100 Zeichen lang sein.")]
    public string Nachname { get; set; } = string.Empty;

    /// <summary>Geburtsjahr (optional).</summary>
    [Range(1000, 9999, ErrorMessage = "Geburtsjahr muss zwischen 1000 und 9999 liegen.")]
    public int? Geburtsjahr { get; set; }

    /// <summary>Wohnort/Bezugsort (optional).</summary>
    public int? OrtId { get; set; }
    public Ort? Ort { get; set; }

    // ---- Computed (nicht gemappt) ----
    /// <summary>Vollständiger Name für Anzeige/Bindestrich.</summary>
    [NotMapped]
    public string VollstaendigerName => string.IsNullOrWhiteSpace(Vorname)
        ? Nachname
        : $"{Vorname} {Nachname}";

    // ---- Navigationen ----
    public List<Buch> Buecher { get; set; } = new();

    public override string ToString() => VollstaendigerName;
}
