using System.ComponentModel.DataAnnotations;

namespace CBAssessment.Models;

public class Section
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }

    public int? TeacherId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
