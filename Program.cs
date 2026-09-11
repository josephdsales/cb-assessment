using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using CBAssessment.Data;
using CBAssessment.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? "NOT_SET";

if (!connectionString.Contains("Trust") && !connectionString.Contains("trust"))
{
    connectionString += ";Trust Server Certificate=true";
}

if (!connectionString.Contains("Pool") && !connectionString.Contains("pool"))
{
    connectionString += ";Maximum Pool Size=3;Connection Idle Lifetime=30;Timeout=30;Command Timeout=30";
}

Console.WriteLine($"DATABASE_URL exists: {!string.IsNullOrEmpty(connectionString)}");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(5);
        npgsqlOptions.CommandTimeout(30);
    }));

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.LogoutPath = "/Home/Logout";
        options.AccessDeniedPath = "/Home/Login";
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Console.WriteLine("Ensuring database exists...");
        db.Database.EnsureCreated();
        Console.WriteLine("Database created successfully");
        SeedData.Initialize(scope.ServiceProvider);
        Console.WriteLine("Seed data initialized");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DATABASE ERROR: {ex.Message}");
        Console.WriteLine($"INNER: {ex.InnerException?.Message}");
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database initialization failed");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/health", () => Results.Ok("healthy"));

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
