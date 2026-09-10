using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CBAssessment.Models;

public enum ExamStatus
{
    Draft,
    Active,
    Closed
}

public class Exam
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public int? SubjectId { get; set; }

    [ForeignKey("SubjectId")]
    public Subject? Subject { get; set; }

    public int TeacherId { get; set; }

    [ForeignKey("TeacherId")]
    public User? Teacher { get; set; }

    public int TimeLimitMinutes { get; set; } = 60;

    public ExamStatus Status { get; set; } = ExamStatus.Draft;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();
}
