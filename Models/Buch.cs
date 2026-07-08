using System.ComponentModel.DataAnnotations;

namespace BibWpf.Models;

/// <summary>
/// Ein Buch (Titel). Hat genau einen Autor, einen Verlag und einen Erscheinungsort.
/// </summary>
public class Buch
{
    public int Id { get; set; }

    /// <summary>Buchtitel.</summary>
    [Required(ErrorMessage = "Titel ist erforderlich.")]
    [StringLength(200, ErrorMessage = "Titel darf max. 200 Zeichen lang sein.")]
    public string Titel { get; set; } = string.Empty;

    /// <summary>ISBN (optional, als String wegen führender Nullen/Prüfziffer).</summary>
    [StringLength(20, MinimumLength = 10,
        ErrorMessage = "ISBN muss 10–20 Zeichen lang sein.")]
    public string? Isbn { get; set; }

    /// <summary>Erscheinungsjahr.</summary>
    [Range(0, 9999, ErrorMessage = "Erscheinungsjahr muss zwischen 0 und 9999 liegen.")]
    public int Erscheinungsjahr { get; set; } = DateTime.UtcNow.Year;

    /// <summary>Anzahl Seiten (optional).</summary>
    [Range(1, 100_000, ErrorMessage = "Seitenanzahl muss zwischen 1 und 100 000 liegen.")]
    public int? Seiten { get; set; }

    // ---- Fremdschlüssel (Beziehungen) ----
    /// <summary>Verfasser:in — Pflichtbeziehung.</summary>
    [Required(ErrorMessage = "Autor ist erforderlich.")]
    public int AutorId { get; set; }
    public Autor? Autor { get; set; }

    /// <summary>Verlag — Pflichtbeziehung.</summary>
    [Required(ErrorMessage = "Verlag ist erforderlich.")]
    public int VerlagId { get; set; }
    public Verlag? Verlag { get; set; }

    public override string ToString()
        => $"{Titel} ({Erscheinungsjahr})";
}
