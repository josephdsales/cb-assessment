using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CBAssessment.Models;

public class StudentAnswer
{
    public int Id { get; set; }

    public int ExamResultId { get; set; }

    [ForeignKey("ExamResultId")]
    public ExamResult? ExamResult { get; set; }

    public int QuestionId { get; set; }

    [ForeignKey("QuestionId")]
    public Question? Question { get; set; }

    [Required]
    [StringLength(5)]
    public string SelectedAnswer { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
