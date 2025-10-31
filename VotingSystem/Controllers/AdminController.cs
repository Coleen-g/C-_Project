using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using VotingSystem.Models;

namespace VotingSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly VotingDbContext _context;

        public AdminController(VotingDbContext context)
        {
            _context = context;
        }

        // ✅ Check if session user is Admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return !string.IsNullOrEmpty(role) && role == "Admin";
        }

        // ✅ Admin Dashboard
        public IActionResult AdminDashboard()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            var totalVoters = _context.Users.Count(u => u.Role == "Voter");
            var totalVotes = _context.Votes.Count();

            var model = new AdminDashboardViewModel
            {
                TotalVoters = totalVoters,
                TotalVotesCast = totalVotes,
                TotalPositions = _context.Positions.Count(),
                Candidates = _context.Candidates.ToList(),
                VotingProgress = totalVoters > 0
                    ? (_context.Votes.Select(v => v.VoterUsername).Distinct().Count() * 100) / totalVoters
                    : 0,
                RecentLogs = _context.UserLogs
                    .OrderByDescending(l => l.Timestamp)
                    .Take(6)
                    .ToList()
            };

            return View("~/Views/Admin/AdminDashboard.cshtml", model);
        }

        // ✅ Add Candidate (GET)
        [HttpGet]
        public IActionResult AddCandidate()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            ViewBag.Positions = _context.Positions.Select(p => p.Name).ToList();
            return View("~/Views/Admin/AddCandidate.cshtml");
        }

        // ✅ Add Candidate (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCandidate(Candidates candidate)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            if (ModelState.IsValid)
            {
                _context.Candidates.Add(candidate);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Candidate added successfully!";
                return RedirectToAction("Candidates");
            }

            ViewBag.Positions = _context.Positions.Select(p => p.Name).ToList();
            return View("~/Views/Admin/AddCandidate.cshtml", candidate);
        }

        // ✅ View Candidates
        public IActionResult Candidates()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            var candidates = _context.Candidates
                .OrderBy(c => c.Position)
                .ToList();

            return View("~/Views/Admin/Candidates.cshtml", candidates);
        }

        // ✅ Edit Candidate (GET)
        [HttpGet]
        public IActionResult EditCandidate(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            var candidate = _context.Candidates.Find(id);
            if (candidate == null)
                return NotFound();

            ViewBag.Positions = _context.Positions.Select(p => p.Name).ToList();
            return View("~/Views/Admin/EditCandidate.cshtml", candidate);
        }

        // ✅ Edit Candidate (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCandidate(Candidates candidate)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            if (ModelState.IsValid)
            {
                _context.Candidates.Update(candidate);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Candidate updated successfully!";
                return RedirectToAction("Candidates");
            }

            ViewBag.Positions = _context.Positions.Select(p => p.Name).ToList();
            return View("~/Views/Admin/EditCandidate.cshtml", candidate);
        }

        // ✅ Delete Candidate
        public IActionResult DeleteCandidate(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            var candidate = _context.Candidates.Find(id);
            if (candidate != null)
            {
                _context.Candidates.Remove(candidate);
                _context.SaveChanges();
            }

            TempData["SuccessMessage"] = "Candidate deleted successfully!";
            return RedirectToAction("Candidates");
        }

        // ✅ View Results
        public IActionResult Results()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            var candidates = _context.Candidates.ToList();
            return View("~/Views/Admin/Results.cshtml", candidates);
        }

        // ✅ View User Logs
        public IActionResult UserLogs()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            var logs = _context.UserLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(200)
                .ToList();

            return View("~/Views/Admin/UserLogs.cshtml", logs);
        }

        // ✅ Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    }
}
