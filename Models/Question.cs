using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CBAssessment.Models;

public enum QuestionType
{
    MultipleChoice,
    TrueFalse,
    FillInBlank,
    LongAnswer,
    MatchingType,
    Ordering
}

public class Question
{
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    public string Text { get; set; } = string.Empty;

    public QuestionType Type { get; set; }

    [StringLength(200)]
    public string? OptionA { get; set; }

    [StringLength(200)]
    public string? OptionB { get; set; }

    [StringLength(200)]
    public string? OptionC { get; set; }

    [StringLength(200)]
    public string? OptionD { get; set; }

    [Required]
    [StringLength(500)]
    public string CorrectAnswer { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? MatchingPairs { get; set; }

    [StringLength(2000)]
    public string? OrderingItems { get; set; }

    [StringLength(2000)]
    public string? SampleAnswer { get; set; }

    public int Points { get; set; } = 1;

    public bool RequiresManualGrading { get; set; }

    public int? ExamId { get; set; }

    [ForeignKey("ExamId")]
    public Exam? Exam { get; set; }

    public int? TeacherId { get; set; }

    [ForeignKey("TeacherId")]
    public User? Teacher { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
