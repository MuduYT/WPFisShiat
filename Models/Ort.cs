using System.ComponentModel.DataAnnotations;

namespace BibWpf.Models;

/// <summary>
/// Eine Stadt/Gemeinde, an der ein Verlag sitzt oder ein Buch verlegt wurde.
/// </summary>
public class Ort
{
    public int Id { get; set; }

    /// <summary>Name der Stadt (z. B. "Berlin", "Stuttgart").</summary>
    [Required(ErrorMessage = "Name ist erforderlich.")]
    [StringLength(120, ErrorMessage = "Name darf max. 120 Zeichen lang sein.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional: Land/Region. Null erlaubt für freie Eingaben.</summary>
    [StringLength(80, ErrorMessage = "Land darf max. 80 Zeichen lang sein.")]
    public string? Land { get; set; }

    // ---- Navigationen ----
    public List<Verlag> Verlage { get; set; } = new();
    public List<Autor> Autoren { get; set; } = new();

    public override string ToString() => string.IsNullOrWhiteSpace(Land)
        ? Name
        : $"{Name}, {Land}";
}
