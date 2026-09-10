using Microsoft.EntityFrameworkCore;
using CBAssessment.Models;

namespace CBAssessment.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();
    public DbSet<StudentAnswer> StudentAnswers => Set<StudentAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Subject>()
            .HasIndex(s => s.Code)
            .IsUnique();
    }
}
