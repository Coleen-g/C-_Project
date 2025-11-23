using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VotingSystem.Models;

namespace VotingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly VotingDbContext _context;

        public HomeController(VotingDbContext context)
        {
            _context = context;
        }

        // ✅ Welcome Page
        public IActionResult Index() => View();

        // ✅ Login (GET)
        [HttpGet]
        public IActionResult Login() => View();

        // ✅ Login (POST)
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                bool validPassword = false;

                if (user.Role == "Admin")
                {
                    using var sha256 = SHA256.Create();
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    string hash = Convert.ToBase64String(bytes);
                    validPassword = hash == user.Password;
                }
                else
                {
                    validPassword = BCrypt.Net.BCrypt.Verify(password, user.Password);
                }

                if (validPassword)
                {
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);

                    if (user.Role == "Admin")
                        return RedirectToAction("AdminDashboard", "Admin");
                    else
                        return RedirectToAction("VoterDashboard", "Voter");
                }
            }

            ViewBag.Error = "Invalid username or password.";
            return View();

        }

        // ✅ Register (GET)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        // ✅ Register (POST)
        // ✅ Register (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string fullName, string username, string password)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            // 🔍 Check if username already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);

            if (existingUser != null)
            {
                ViewBag.Error = "Voter already registered!";
                return View();   // stays on Register page and shows message
            }

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                FullName = fullName,
                Username = username,
                Password = hashedPassword,
                Role = "Voter"
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            LogUserAction(newUser.Username, "Registered new account");

            // ✅ Redirect to Login page after successful registration
            TempData["SuccessMessage"] = "Registration successful! Please login.";
            return RedirectToAction("Login");
        }






        // ✅ Logout
        public IActionResult Logout()
        {
            var username = HttpContext.Session.GetString("Username");

            // ✅ Log user logout
            if (!string.IsNullOrEmpty(username))
                LogUserAction(username, "Logged out");

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ✅ Helper method to log user actions
        private void LogUserAction(string username, string action)
        {
            var log = new UserLog
            {
                VoterUsername = username,
                Action = action,
                Timestamp = DateTime.Now
            };

            _context.UserLogs.Add(log);
            _context.SaveChanges();
        }
    }
}
