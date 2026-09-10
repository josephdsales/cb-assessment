using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CBAssessment.Data;
using CBAssessment.Models;

namespace CBAssessment.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    public int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public async Task<IActionResult> Dashboard()
    {
        ViewBag.TotalTeachers = await _context.Users.CountAsync(u => u.Role == UserRole.Teacher);
        ViewBag.TotalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student);
        ViewBag.TotalExams = await _context.Exams.CountAsync();
        ViewBag.TotalSections = await _context.Sections.CountAsync();
        return View();
    }

    // Teacher Management
    public async Task<IActionResult> ManageTeachers()
    {
        var teachers = await _context.Users
            .Where(u => u.Role == UserRole.Teacher)
            .OrderBy(u => u.FullName)
            .ToListAsync();
        return View(teachers);
    }

    [HttpGet]
    public IActionResult CreateTeacher()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateTeacher(string fullName, string username, string password, string confirmPassword)
    {
        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "All fields are required.";
            return View();
        }

        if (password != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match.";
            return View();
        }

        if (password.Length < 6)
        {
            ViewBag.Error = "Password must be at least 6 characters.";
            return View();
        }

        if (await _context.Users.AnyAsync(u => u.Username == username))
        {
            ViewBag.Error = "Username already exists.";
            return View();
        }

        var teacher = new User
        {
            FullName = fullName,
            Username = username,
            PasswordHash = BCryptHelper.HashPassword(password),
            Role = UserRole.Teacher,
            CreatedAt = DateTime.Now
        };

        _context.Users.Add(teacher);
        await _context.SaveChangesAsync();

        ViewBag.Success = "Teacher account created successfully!";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTeacher(int id)
    {
        var teacher = await _context.Users.FindAsync(id);
        if (teacher != null && teacher.Role == UserRole.Teacher)
        {
            _context.Users.Remove(teacher);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("ManageTeachers");
    }

    // Student Management
    public async Task<IActionResult> ManageStudents()
    {
        var students = await _context.Users
            .Where(u => u.Role == UserRole.Student)
            .OrderBy(u => u.FullName)
            .ToListAsync();
        return View(students);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var student = await _context.Users.FindAsync(id);
        if (student != null && student.Role == UserRole.Student)
        {
            _context.Users.Remove(student);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("ManageStudents");
    }

    // Section Management
    public async Task<IActionResult> ManageSections()
    {
        var sections = await _context.Sections
            .OrderBy(s => s.Name)
            .ToListAsync();
        return View(sections);
    }

    [HttpGet]
    public IActionResult CreateSection()
    {
        return View(new Section());
    }

    [HttpPost]
    public async Task<IActionResult> CreateSection(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ViewBag.Error = "Section name is required.";
            return View(new Section());
        }

        var exists = await _context.Sections.AnyAsync(s => s.Name == name);
        if (exists)
        {
            ViewBag.Error = "A section with this name already exists.";
            return View(new Section { Name = name, Description = description });
        }

        var section = new Section
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            CreatedAt = DateTime.Now
        };
        _context.Sections.Add(section);
        await _context.SaveChangesAsync();
        return RedirectToAction("ManageSections");
    }

    [HttpGet]
    public async Task<IActionResult> EditSection(int id)
    {
        var section = await _context.Sections.FindAsync(id);
        if (section == null) return NotFound();
        return View(section);
    }

    [HttpPost]
    public async Task<IActionResult> EditSection(int id, string name, string? description)
    {
        var section = await _context.Sections.FindAsync(id);
        if (section == null) return NotFound();

        if (string.IsNullOrWhiteSpace(name))
        {
            ViewBag.Error = "Section name is required.";
            return View(section);
        }

        section.Name = name.Trim();
        section.Description = description?.Trim();
        await _context.SaveChangesAsync();
        return RedirectToAction("ManageSections");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSection(int id)
    {
        var section = await _context.Sections.FindAsync(id);
        if (section != null)
        {
            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("ManageSections");
    }
}
