using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CBAssessment.Data;
using CBAssessment.Models;

namespace CBAssessment.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly AppDbContext _context;

    public TeacherController(AppDbContext context)
    {
        _context = context;
    }

    public int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public async Task<IActionResult> Dashboard()
    {
        var teacherId = GetUserId();
        var exams = await _context.Exams
            .Include(e => e.Subject)
            .Include(e => e.Questions)
            .Include(e => e.Results)
            .Where(e => e.TeacherId == teacherId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        ViewBag.TotalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student);
        ViewBag.TotalExams = exams.Count;
        ViewBag.TotalResults = await _context.ExamResults
            .Include(r => r.Exam)
            .CountAsync(r => r.Exam!.TeacherId == teacherId);

        return View(exams);
    }

    [HttpGet]
    public async Task<IActionResult> CreateExam()
    {
        ViewBag.Subjects = await _context.Subjects.ToListAsync();
        return View(new Exam());
    }

    [HttpPost]
    public async Task<IActionResult> CreateExam(Exam exam)
    {
        exam.TeacherId = GetUserId();
        exam.CreatedAt = DateTime.Now;
        exam.Status = ExamStatus.Draft;
        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();
        return RedirectToAction("AddQuestions", new { examId = exam.Id });
    }

    [HttpGet]
    public async Task<IActionResult> AddQuestions(int examId)
    {
        var exam = await _context.Exams.FindAsync(examId);
        if (exam == null) return NotFound();
        ViewBag.ExamId = examId;
        ViewBag.ExamTitle = exam.Title;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddQuestions(int examId, string questionText, QuestionType type,
        string optionA, string optionB, string? optionC, string? optionD,
        string correctAnswer, int points)
    {
        var question = new Question
        {
            Text = questionText,
            Type = type,
            OptionA = optionA,
            OptionB = optionB,
            OptionC = optionC,
            OptionD = optionD,
            CorrectAnswer = correctAnswer,
            Points = points,
            ExamId = examId,
            TeacherId = GetUserId(),
            CreatedAt = DateTime.Now
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        ViewBag.ExamId = examId;
        ViewBag.ExamTitle = (await _context.Exams.FindAsync(examId))?.Title;
        ViewBag.Success = "Question added successfully!";

        var existingQuestions = await _context.Questions.Where(q => q.ExamId == examId).ToListAsync();
        ViewBag.ExistingQuestions = existingQuestions;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ViewQuestions(int examId)
    {
        var exam = await _context.Exams
            .Include(e => e.Questions)
            .Include(e => e.Subject)
            .FirstOrDefaultAsync(e => e.Id == examId && e.TeacherId == GetUserId());

        if (exam == null) return NotFound();
        return View(exam);
    }

    [HttpPost]
    public async Task<IActionResult> ActivateExam(int examId)
    {
        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == examId && e.TeacherId == GetUserId());
        if (exam != null)
        {
            exam.Status = ExamStatus.Active;
            exam.StartTime = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> CloseExam(int examId)
    {
        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == examId && e.TeacherId == GetUserId());
        if (exam != null)
        {
            exam.Status = ExamStatus.Closed;
            exam.EndTime = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteExam(int examId)
    {
        var exam = await _context.Exams
            .Include(e => e.Questions)
            .Include(e => e.Results)
            .FirstOrDefaultAsync(e => e.Id == examId && e.TeacherId == GetUserId());

        if (exam != null)
        {
            _context.Questions.RemoveRange(exam.Questions);
            _context.ExamResults.RemoveRange(exam.Results);
            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Dashboard");
    }

    [HttpGet]
    public async Task<IActionResult> EditQuestion(int questionId)
    {
        var question = await _context.Questions
            .Include(q => q.Exam)
            .FirstOrDefaultAsync(q => q.Id == questionId && q.TeacherId == GetUserId());

        if (question == null) return NotFound();
        return View(question);
    }

    [HttpPost]
    public async Task<IActionResult> EditQuestion(int id, string questionText, QuestionType type,
        string optionA, string optionB, string? optionC, string? optionD,
        string correctAnswer, int points)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null) return NotFound();

        question.Text = questionText;
        question.Type = type;
        question.OptionA = optionA;
        question.OptionB = optionB;
        question.OptionC = optionC;
        question.OptionD = optionD;
        question.CorrectAnswer = correctAnswer;
        question.Points = points;

        await _context.SaveChangesAsync();
        return RedirectToAction("ViewQuestions", new { examId = question.ExamId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteQuestion(int questionId, int examId)
    {
        var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == questionId && q.TeacherId == GetUserId());
        if (question != null)
        {
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("ViewQuestions", new { examId });
    }

    public async Task<IActionResult> Reports()
    {
        var teacherId = GetUserId();
        var results = await _context.ExamResults
            .Include(r => r.Exam)
            .Include(r => r.Student)
            .Where(r => r.Exam!.TeacherId == teacherId)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();

        ViewBag.Exams = await _context.Exams
            .Where(e => e.TeacherId == teacherId)
            .ToListAsync();

        return View(results);
    }

    public async Task<IActionResult> ExamReport(int examId)
    {
        var teacherId = GetUserId();
        var exam = await _context.Exams
            .Include(e => e.Subject)
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == examId && e.TeacherId == teacherId);

        if (exam == null) return NotFound();

        var results = await _context.ExamResults
            .Include(r => r.Student)
            .Include(r => r.Exam)
            .Where(r => r.ExamId == examId)
            .OrderByDescending(r => r.Score)
            .ToListAsync();

        ViewBag.Exam = exam;
        ViewBag.AverageScore = results.Any() ? results.Average(r => (double)r.Score / r.TotalPoints * 100) : 0;
        ViewBag.HighestScore = results.Any() ? results.Max(r => (double)r.Score / r.TotalPoints * 100) : 0;
        ViewBag.LowestScore = results.Any() ? results.Min(r => (double)r.Score / r.TotalPoints * 100) : 0;
        ViewBag.TotalStudents = results.Count;

        return View(results);
    }

    [HttpGet]
    public async Task<IActionResult> ManageStudents()
    {
        var students = await _context.Users
            .Where(u => u.Role == UserRole.Student)
            .OrderBy(u => u.FullName)
            .ToListAsync();
        return View(students);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteStudent(int studentId)
    {
        var student = await _context.Users.FindAsync(studentId);
        if (student != null)
        {
            _context.Users.Remove(student);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("ManageStudents");
    }
}
