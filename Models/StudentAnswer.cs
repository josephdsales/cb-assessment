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

    [StringLength(500)]
    public string SelectedAnswer { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? LongAnswerText { get; set; }

    public bool IsCorrect { get; set; }

    public bool IsPendingReview { get; set; }

    public int? AwardedPoints { get; set; }
}
