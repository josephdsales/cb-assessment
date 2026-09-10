using CBAssessment.Models;
using Microsoft.EntityFrameworkCore;

namespace CBAssessment.Data;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var context = new AppDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

        if (context.Users.Any())
            return;

        var teacherHash = BCryptHelper.HashPassword("teacher123");
        var studentHash = BCryptHelper.HashPassword("student123");
        var adminHash = BCryptHelper.HashPassword("admin123");

        var admin = new User
        {
            FullName = "System Administrator",
            Username = "admin",
            PasswordHash = adminHash,
            Role = UserRole.Admin,
            CreatedAt = DateTime.Now
        };

        var teacher = new User
        {
            FullName = "Prof. Admin",
            Username = "teacher",
            PasswordHash = teacherHash,
            Role = UserRole.Teacher,
            CreatedAt = DateTime.Now
        };

        var students = new List<User>
        {
            new User { FullName = "John Smith", Username = "john", PasswordHash = studentHash, Role = UserRole.Student, ClassSection = "10-A" },
            new User { FullName = "Jane Doe", Username = "jane", PasswordHash = studentHash, Role = UserRole.Student, ClassSection = "10-A" },
            new User { FullName = "Bob Johnson", Username = "bob", PasswordHash = studentHash, Role = UserRole.Student, ClassSection = "10-B" },
            new User { FullName = "Alice Brown", Username = "alice", PasswordHash = studentHash, Role = UserRole.Student, ClassSection = "10-B" },
            new User { FullName = "Charlie Wilson", Username = "charlie", PasswordHash = studentHash, Role = UserRole.Student, ClassSection = "11-A" }
        };

        context.Users.Add(admin);
        context.Users.Add(teacher);
        context.Users.AddRange(students);
        context.SaveChanges();

        var subjects = new List<Subject>
        {
            new Subject { Name = "Filipino", Code = "FIL101" },
            new Subject { Name = "English", Code = "ENG101" },
            new Subject { Name = "Mathematics", Code = "MATH101" },
            new Subject { Name = "Science", Code = "SCI101" },
            new Subject { Name = "Araling Panlipunan", Code = "AP101" },
            new Subject { Name = "Values Education", Code = "VE101" },
            new Subject { Name = "TLE", Code = "TLE101" },
            new Subject { Name = "MAPEH", Code = "MAPEH101" },
            new Subject { Name = "Add-ons", Code = "ADD101" }
        };
        context.Subjects.AddRange(subjects);
        context.SaveChanges();

        var mathExam = new Exam
        {
            Title = "Mathematics Mid-Term Exam",
            Description = "Covers algebra, geometry, and basic calculus concepts.",
            SubjectId = subjects[0].Id,
            TeacherId = teacher.Id,
            TimeLimitMinutes = 30,
            Status = ExamStatus.Active,
            StartTime = DateTime.Now.AddDays(-1),
            EndTime = DateTime.Now.AddDays(7)
        };
        context.Exams.Add(mathExam);
        context.SaveChanges();

        var mathQuestions = new List<Question>
        {
            new Question { Text = "What is 2 + 2?", Type = QuestionType.MultipleChoice, OptionA = "3", OptionB = "4", OptionC = "5", OptionD = "6", CorrectAnswer = "B", ExamId = mathExam.Id, TeacherId = teacher.Id, Points = 2 },
            new Question { Text = "What is the square root of 16?", Type = QuestionType.MultipleChoice, OptionA = "2", OptionB = "3", OptionC = "4", OptionD = "8", CorrectAnswer = "C", ExamId = mathExam.Id, TeacherId = teacher.Id, Points = 2 },
            new Question { Text = "Solve for x: 2x = 10", Type = QuestionType.MultipleChoice, OptionA = "3", OptionB = "4", OptionC = "5", OptionD = "6", CorrectAnswer = "C", ExamId = mathExam.Id, TeacherId = teacher.Id, Points = 2 },
            new Question { Text = "What is 15% of 200?", Type = QuestionType.MultipleChoice, OptionA = "25", OptionB = "30", OptionC = "35", OptionD = "40", CorrectAnswer = "B", ExamId = mathExam.Id, TeacherId = teacher.Id, Points = 2 },
            new Question { Text = "A triangle has angles of 60° and 80°. What is the third angle?", Type = QuestionType.MultipleChoice, OptionA = "30°", OptionB = "40°", OptionC = "50°", OptionD = "60°", CorrectAnswer = "B", ExamId = mathExam.Id, TeacherId = teacher.Id, Points = 2 },
            new Question { Text = "Is 0.5 greater than 0.25?", Type = QuestionType.TrueFalse, OptionA = "True", OptionB = "False", CorrectAnswer = "A", ExamId = mathExam.Id, TeacherId = teacher.Id, Points = 1 },
            new Question { Text = "Is -3 a positive number?", Type = QuestionType.TrueFalse, OptionA = "True", OptionB = "False", CorrectAnswer = "B", ExamId = mathExam.Id, TeacherId = teacher.Id, Points = 1 },
            new Question { Text = "What is 7 × 8?", Type = QuestionType.MultipleChoice, OptionA = "48", OptionB = "54", OptionC = "56", OptionD = "63", CorrectAnswer = "C", ExamId = mathExam.Id, TeacherId = teacher.Id, Points = 2 },
            new Question { Text = "What is the area of a square with side 5?", Type = QuestionType.MultipleChoice, OptionA = "10", OptionB = "15", OptionC = "20", OptionD = "25", CorrectAnswer = "D", ExamId = mathExam.Id, TeacherId = teacher.Id, Points = 2 },
            new Question { Text = "What is 100 ÷ 4?", Type = QuestionType.MultipleChoice, OptionA = "20", OptionB = "25", OptionC = "30", OptionD = "40", CorrectAnswer = "B", ExamId = mathExam.Id, TeacherId = teacher.Id, Points = 2 }
        };
        context.Questions.AddRange(mathQuestions);
        context.SaveChanges();

        var engExam = new Exam
        {
            Title = "English Grammar Quiz",
            Description = "Basic grammar and vocabulary assessment.",
            SubjectId = subjects[1].Id,
            TeacherId = teacher.Id,
            TimeLimitMinutes = 20,
            Status = ExamStatus.Active,
            StartTime = DateTime.Now.AddDays(-1),
            EndTime = DateTime.Now.AddDays(7)
        };
        context.Exams.Add(engExam);
        context.SaveChanges();

        var engQuestions = new List<Question>
        {
            new Question { Text = "Which word is a noun?", Type = QuestionType.MultipleChoice, OptionA = "Run", OptionB = "Happy", OptionC = "Dog", OptionD = "Quickly", CorrectAnswer = "C", ExamId = engExam.Id, TeacherId = teacher.Id, Points = 2 },
            new Question { Text = "Choose the correct form: She ___ to school.", Type = QuestionType.MultipleChoice, OptionA = "go", OptionB = "goes", OptionC = "going", OptionD = "gone", CorrectAnswer = "B", ExamId = engExam.Id, TeacherId = teacher.Id, Points = 2 },
            new Question { Text = "Is 'beautiful' an adjective?", Type = QuestionType.TrueFalse, OptionA = "True", OptionB = "False", CorrectAnswer = "A", ExamId = engExam.Id, TeacherId = teacher.Id, Points = 1 },
            new Question { Text = "What is the past tense of 'run'?", Type = QuestionType.MultipleChoice, OptionA = "Runned", OptionB = "Ran", OptionC = "Running", OptionD = "Runs", CorrectAnswer = "B", ExamId = engExam.Id, TeacherId = teacher.Id, Points = 2 },
            new Question { Text = "Which is a pronoun?", Type = QuestionType.MultipleChoice, OptionA = "Big", OptionB = "And", OptionC = "She", OptionD = "Jump", CorrectAnswer = "C", ExamId = engExam.Id, TeacherId = teacher.Id, Points = 2 }
        };
        context.Questions.AddRange(engQuestions);
        context.SaveChanges();

        var scienceExam = new Exam
        {
            Title = "Science Mixed Quiz",
            Description = "A quiz with different question types.",
            SubjectId = subjects[3].Id,
            TeacherId = teacher.Id,
            TimeLimitMinutes = 25,
            Status = ExamStatus.Active,
            StartTime = DateTime.Now.AddDays(-1),
            EndTime = DateTime.Now.AddDays(7)
        };
        context.Exams.Add(scienceExam);
        context.SaveChanges();

        var scienceQuestions = new List<Question>
        {
            new Question
            {
                Text = "What is the chemical symbol for water?",
                Type = QuestionType.FillInBlank,
                CorrectAnswer = "H2O|h2o",
                Points = 2,
                ExamId = scienceExam.Id,
                TeacherId = teacher.Id
            },
            new Question
            {
                Text = "Explain the process of photosynthesis.",
                Type = QuestionType.LongAnswer,
                CorrectAnswer = "MANUAL",
                SampleAnswer = "Photosynthesis is the process by which green plants use sunlight, water, and carbon dioxide to produce glucose and oxygen. It occurs in the chloroplasts of plant cells using chlorophyll.",
                RequiresManualGrading = true,
                Points = 5,
                ExamId = scienceExam.Id,
                TeacherId = teacher.Id
            },
            new Question
            {
                Text = "Match the planet with its characteristic.",
                Type = QuestionType.MatchingType,
                MatchingPairs = "{\"Left\":[\"1. Mercury\",\"2. Venus\",\"3. Earth\",\"4. Mars\"],\"Right\":[\"A. Closest to Sun\",\"B. Hottest planet\",\"C. Has life\",\"D. Red planet\"]}",
                CorrectAnswer = "ABCD",
                Points = 4,
                ExamId = scienceExam.Id,
                TeacherId = teacher.Id
            },
            new Question
            {
                Text = "Arrange the planets from the Sun outward (first 4).",
                Type = QuestionType.Ordering,
                OrderingItems = "[\"Mercury\",\"Venus\",\"Earth\",\"Mars\"]",
                CorrectAnswer = "1234",
                Points = 4,
                ExamId = scienceExam.Id,
                TeacherId = teacher.Id
            },
            new Question
            {
                Text = "The powerhouse of the cell is the ___.",
                Type = QuestionType.FillInBlank,
                CorrectAnswer = "mitochondria|Mitochondria|MITOCHONDRIA",
                Points = 2,
                ExamId = scienceExam.Id,
                TeacherId = teacher.Id
            },
            new Question
            {
                Text = "Explain the difference between weather and climate.",
                Type = QuestionType.LongAnswer,
                CorrectAnswer = "MANUAL",
                SampleAnswer = "Weather refers to short-term atmospheric conditions (days/weeks), while climate refers to long-term patterns of weather in a region (30+ years).",
                RequiresManualGrading = true,
                Points = 5,
                ExamId = scienceExam.Id,
                TeacherId = teacher.Id
            }
        };
        context.Questions.AddRange(scienceQuestions);
        context.SaveChanges();
    }
}

public static class BCryptHelper
{
    public static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "CBAssessmentSalt2024"));
        return Convert.ToBase64String(bytes);
    }

    public static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
