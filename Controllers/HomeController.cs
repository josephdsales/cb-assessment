using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CBAssessment.Data;
using CBAssessment.Models;

namespace CBAssessment.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToAction("Login");

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role == "Teacher")
            return RedirectToAction("Dashboard", "Teacher");
        else
            return RedirectToAction("Dashboard", "Student");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Username and password are required.";
            return View();
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null || !BCryptHelper.VerifyPassword(password, user.PasswordHash))
        {
            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("Username", user.Username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        ViewBag.Sections = await _context.Sections.OrderBy(s => s.Name).ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string fullName, string username, string password, string confirmPassword, string? classSection)
    {
        ViewBag.Sections = await _context.Sections.OrderBy(s => s.Name).ToListAsync();

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

        var user = new User
        {
            FullName = fullName,
            Username = username,
            PasswordHash = BCryptHelper.HashPassword(password),
            Role = UserRole.Student,
            ClassSection = classSection
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        ViewBag.Success = "Registration successful! You can now login.";
        return View("Login");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
