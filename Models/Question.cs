using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CBAssessment.Models;

public enum QuestionType
{
    MultipleChoice,
    TrueFalse
}

public class Question
{
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    public string Text { get; set; } = string.Empty;

    public QuestionType Type { get; set; }

    [Required]
    [StringLength(200)]
    public string OptionA { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string OptionB { get; set; } = string.Empty;

    [StringLength(200)]
    public string? OptionC { get; set; }

    [StringLength(200)]
    public string? OptionD { get; set; }

    [Required]
    [StringLength(5)]
    public string CorrectAnswer { get; set; } = string.Empty;

    public int Points { get; set; } = 1;

    public int? ExamId { get; set; }

    [ForeignKey("ExamId")]
    public Exam? Exam { get; set; }

    public int? TeacherId { get; set; }

    [ForeignKey("TeacherId")]
    public User? Teacher { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
