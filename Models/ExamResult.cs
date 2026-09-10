using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CBAssessment.Models;

public class ExamResult
{
    public int Id { get; set; }

    public int ExamId { get; set; }

    [ForeignKey("ExamId")]
    public Exam? Exam { get; set; }

    public int StudentId { get; set; }

    [ForeignKey("StudentId")]
    public User? Student { get; set; }

    public int Score { get; set; }

    public int TotalPoints { get; set; }

    public int CorrectAnswers { get; set; }

    public int TotalQuestions { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.Now;

    public TimeSpan? TimeTaken { get; set; }
}
