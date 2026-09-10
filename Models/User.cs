using System.ComponentModel.DataAnnotations;

namespace CBAssessment.Models;

public enum UserRole
{
    Student,
    Teacher,
    Admin
}

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    [StringLength(100)]
    public string? ClassSection { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Exam> CreatedExams { get; set; } = new List<Exam>();
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
}
