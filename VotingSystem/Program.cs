using Microsoft.EntityFrameworkCore;
using VotingSystem.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// ✅ Add MVC
// ==========================
builder.Services.AddControllersWithViews();

// ==========================
// ✅ Add Session support
// ==========================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // Allow sending cookies on localhost HTTP for development
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ==========================
// ✅ Configure antiforgery cookies
// ==========================
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // Allow sending antiforgery cookie on HTTP for dev
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ==========================
// ✅ Connect to MySQL
// ==========================
builder.Services.AddDbContext<VotingDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    ));

// ==========================
// Build App
// ==========================
var app = builder.Build();

// ==========================
// ✅ Apply database migrations automatically on startup
// ==========================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VotingDbContext>();

    // Apply migrations (creates/updates tables automatically)
    context.Database.Migrate();

    // ✅ Seed Positions if database is empty
    if (!context.Positions.Any())
    {
        context.Positions.AddRange(
            new Position { Name = "President" },
            new Position { Name = "Vice President" },
            new Position { Name = "Secretary" },
            new Position { Name = "Treasurer" },
            new Position { Name = "Auditor" }
        );
        context.SaveChanges();
    }
}

// ==========================
// ✅ Middleware pipeline
// ==========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

// ==========================
// ✅ Force HTTPS redirection
// ==========================
app.UseHttpsRedirection();

// ==========================
// ✅ Enable session
// ==========================
app.UseSession();

// ==========================
// ✅ Routing & Authorization
// ==========================
app.UseRouting();
app.UseAuthorization();

// ==========================
// ✅ Default route
// ==========================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
