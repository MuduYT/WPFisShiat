using System.ComponentModel.DataAnnotations;

namespace BibWpf.Models;

/// <summary>
/// Ein Verlag, der Bücher herausgibt.
/// </summary>
public class Verlag
{
    public int Id { get; set; }

    /// <summary>Firma/Name des Verlags (z. B. "Suhrkamp Verlag").</summary>
    [Required(ErrorMessage = "Name ist erforderlich.")]
    [StringLength(150, ErrorMessage = "Name darf max. 150 Zeichen lang sein.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gründungsjahr (optional).</summary>
    [Range(1000, 9999, ErrorMessage = "Gründungsjahr muss zwischen 1000 und 9999 liegen.")]
    public int? Gruendungsjahr { get; set; }

    /// <summary>Sitz des Verlags.</summary>
    public int? OrtId { get; set; }
    public Ort? Ort { get; set; }

    // ---- Navigationen ----
    public List<Buch> Buecher { get; set; } = new();

    public override string ToString() => Name;
}
