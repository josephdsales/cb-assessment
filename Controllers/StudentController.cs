using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CBAssessment.Data;
using CBAssessment.Models;

namespace CBAssessment.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly AppDbContext _context;

    public StudentController(AppDbContext context)
    {
        _context = context;
    }

    public int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public async Task<IActionResult> Dashboard()
    {
        var studentId = GetUserId();
        var student = await _context.Users.FindAsync(studentId);
        var studentSection = student?.ClassSection ?? "";

        var availableExams = await _context.Exams
            .Include(e => e.Subject)
            .Include(e => e.Questions)
            .Where(e => e.Status == ExamStatus.Active &&
                (string.IsNullOrEmpty(e.TargetSections) || e.TargetSections.Contains(studentSection)))
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        var completedResults = await _context.ExamResults
            .Include(r => r.Exam)
            .ThenInclude(e => e!.Subject)
            .Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();

        var completedExamIds = completedResults.Select(r => r.ExamId).ToHashSet();
        var pendingExams = availableExams.Where(e => !completedExamIds.Contains(e.Id)).ToList();

        ViewBag.AvailableExams = pendingExams;
        ViewBag.CompletedResults = completedResults;
        ViewBag.Student = student;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> TakeExam(int examId)
    {
        var studentId = GetUserId();
        var exam = await _context.Exams
            .Include(e => e.Questions)
            .Include(e => e.Subject)
            .FirstOrDefaultAsync(e => e.Id == examId && e.Status == ExamStatus.Active);

        if (exam == null) return RedirectToAction("Dashboard");

        var alreadyTaken = await _context.ExamResults.AnyAsync(r => r.ExamId == examId && r.StudentId == studentId);
        if (alreadyTaken)
        {
            TempData["Message"] = "You have already taken this exam.";
            return RedirectToAction("Dashboard");
        }

        return View(exam);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitExam(int examId, Dictionary<int, string> answers,
        Dictionary<int, string>? longAnswers)
    {
        var studentId = GetUserId();
        var exam = await _context.Exams
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == examId && e.Status == ExamStatus.Active);

        if (exam == null) return RedirectToAction("Dashboard");

        int score = 0;
        int totalPoints = exam.Questions.Sum(q => q.Points);
        var studentAnswers = new List<StudentAnswer>();

        foreach (var question in exam.Questions)
        {
            var sa = new StudentAnswer
            {
                QuestionId = question.Id,
                SelectedAnswer = "",
                LongAnswerText = null,
                IsCorrect = false,
                IsPendingReview = false,
                AwardedPoints = null
            };

            switch (question.Type)
            {
                case QuestionType.MultipleChoice:
                case QuestionType.TrueFalse:
                    var selected = answers.ContainsKey(question.Id) ? answers[question.Id] ?? "" : "";
                    sa.SelectedAnswer = selected;
                    sa.IsCorrect = selected == question.CorrectAnswer;
                    if (sa.IsCorrect) score += question.Points;
                    break;

                case QuestionType.FillInBlank:
                    var fibAnswer = answers.ContainsKey(question.Id) ? answers[question.Id] ?? "" : "";
                    sa.SelectedAnswer = fibAnswer;
                    var acceptedAnswers = question.CorrectAnswer.Split('|', StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => a.Trim().ToLower()).ToArray();
                    sa.IsCorrect = acceptedAnswers.Contains(fibAnswer.Trim().ToLower());
                    if (sa.IsCorrect) score += question.Points;
                    break;

                case QuestionType.LongAnswer:
                    var longText = longAnswers != null && longAnswers.ContainsKey(question.Id)
                        ? longAnswers[question.Id] ?? "" : "";
                    sa.LongAnswerText = longText;
                    sa.IsPendingReview = true;
                    sa.IsCorrect = false;
                    break;

                case QuestionType.MatchingType:
                    var matchAnswer = answers.ContainsKey(question.Id) ? answers[question.Id] ?? "" : "";
                    sa.SelectedAnswer = matchAnswer;
                    sa.IsCorrect = string.Equals(matchAnswer, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
                    if (sa.IsCorrect) score += question.Points;
                    break;

                case QuestionType.Ordering:
                    var orderAnswer = answers.ContainsKey(question.Id) ? answers[question.Id] ?? "" : "";
                    sa.SelectedAnswer = orderAnswer;
                    sa.IsCorrect = orderAnswer == question.CorrectAnswer;
                    if (sa.IsCorrect) score += question.Points;
                    break;
            }

            studentAnswers.Add(sa);
        }

        var hasManualGrading = studentAnswers.Any(sa => sa.IsPendingReview);

        var examResult = new ExamResult
        {
            ExamId = examId,
            StudentId = studentId,
            Score = score,
            TotalPoints = totalPoints,
            CorrectAnswers = studentAnswers.Count(a => a.IsCorrect),
            TotalQuestions = exam.Questions.Count,
            SubmittedAt = DateTime.Now
        };

        _context.ExamResults.Add(examResult);
        await _context.SaveChangesAsync();

        foreach (var sa in studentAnswers)
        {
            sa.ExamResultId = examResult.Id;
        }
        _context.StudentAnswers.AddRange(studentAnswers);
        await _context.SaveChangesAsync();

        return RedirectToAction("ViewResult", new { resultId = examResult.Id });
    }

    public async Task<IActionResult> ViewResult(int resultId)
    {
        var studentId = GetUserId();
        var result = await _context.ExamResults
            .Include(r => r.Exam)
                .ThenInclude(e => e!.Subject)
            .Include(r => r.Exam)
                .ThenInclude(e => e!.Questions)
            .FirstOrDefaultAsync(r => r.Id == resultId && r.StudentId == studentId);

        if (result == null) return RedirectToAction("Dashboard");

        var studentAnswers = await _context.StudentAnswers
            .Include(sa => sa.Question)
            .Where(sa => sa.ExamResultId == resultId)
            .ToListAsync();

        ViewBag.StudentAnswers = studentAnswers;
        ViewBag.Percentage = result.TotalPoints > 0 ? (double)result.Score / result.TotalPoints * 100 : 0;
        ViewBag.HasPendingReview = studentAnswers.Any(sa => sa.IsPendingReview);

        return View(result);
    }

    public async Task<IActionResult> MyScores()
    {
        var studentId = GetUserId();
        var results = await _context.ExamResults
            .Include(r => r.Exam)
                .ThenInclude(e => e!.Subject)
            .Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();

        return View(results);
    }
}
