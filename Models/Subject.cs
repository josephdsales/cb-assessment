using System.ComponentModel.DataAnnotations;

namespace CBAssessment.Models;

public class Subject
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Code { get; set; }

    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
