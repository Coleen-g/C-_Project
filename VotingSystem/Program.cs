using Microsoft.EntityFrameworkCore;
using VotingSystem.Models; // contains VotingDbContext
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✅ Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Connect to MySQL
builder.Services.AddDbContext<VotingDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    ));

var app = builder.Build();

// ✅ Seed positions and ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VotingDbContext>();

    // Ensure database is created
    context.Database.EnsureCreated();

    // Seed positions if empty
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

// ✅ Enable session
app.UseSession();

app.UseAuthorization();

// Default route — open Home/Index (Welcome page) first
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
