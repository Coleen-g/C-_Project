using Microsoft.AspNetCore.Mvc;
using VotingSystem.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace VotingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly VotingDbContext _context;

        public HomeController(VotingDbContext context)
        {
            _context = context;
        }

        // ✅ Welcome page
        public IActionResult Index()
        {
            return View();
        }

        // ✅ Login (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ✅ Login (POST)
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                if (user.Role == "Admin")
                    return RedirectToAction("AdminDashboard");
                else
                    return RedirectToAction("VoterDashboard");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        // ✅ Admin dashboard
        public IActionResult AdminDashboard()
        {
            if (HttpContext.Session.GetString("Username") == null ||
                HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login");

            return View();
        }

        // ✅ Add candidate (GET)
        [HttpGet]
        public IActionResult AddCandidate()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login");

            // Get positions from DB dynamically
            ViewBag.Positions = _context.Positions.Select(p => p.Name).ToList();

            return View();
        }

        // ✅ Add candidate (POST)
        [HttpPost]
        public IActionResult AddCandidate(Candidates candidate)
        {
            if (ModelState.IsValid)
            {
                _context.Candidates.Add(candidate);
                _context.SaveChanges();
                return RedirectToAction("Candidates");
            }
            return View(candidate);
        }

        // ✅ View all candidates (grouped by position)
        public IActionResult Candidates()
        {
            if (HttpContext.Session.GetString("Username") == null ||
                HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login");

            var candidates = _context.Candidates.ToList();
            return View(candidates);
        }

        // ✅ Edit candidate (GET)
        [HttpGet]
        public IActionResult EditCandidate(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login");

            var candidate = _context.Candidates.Find(id);
            if (candidate == null) return NotFound();

            // Populate positions dynamically
            ViewBag.Positions = _context.Positions.Select(p => p.Name).ToList();

            return View(candidate);
        }

        // ✅ Edit candidate (POST)
        [HttpPost]
        public IActionResult EditCandidate(Candidates candidate)
        {
            if (ModelState.IsValid)
            {
                _context.Candidates.Update(candidate);
                _context.SaveChanges();
                return RedirectToAction("Candidates");
            }

            ViewBag.Positions = _context.Positions.Select(p => p.Name).ToList();
            return View(candidate);
        }

        // ✅ Delete candidate
        public IActionResult DeleteCandidate(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login");

            var candidate = _context.Candidates.Find(id);
            if (candidate != null)
            {
                _context.Candidates.Remove(candidate);
                _context.SaveChanges();
            }
            return RedirectToAction("Candidates");
        }



        // Display candidates to voter
        // Display candidates to voter
        public IActionResult Vote()
        {
            if (HttpContext.Session.GetString("Username") == null ||
                HttpContext.Session.GetString("Role") != "Voter")
                return RedirectToAction("Login");

            var candidates = _context.Candidates.ToList();

            // Get list of positions the voter already voted for
            var username = HttpContext.Session.GetString("Username");
            ViewBag.VotedPositions = _context.Votes
                .Where(v => v.VoterUsername == username)
                .Select(v => v.Position)
                .ToList();

            return View(candidates);
        }

        // Process vote
        [HttpPost]
        public IActionResult Vote(int candidateId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login");

            var candidate = _context.Candidates.Find(candidateId);
            if (candidate == null)
                return NotFound();

            // Check if user already voted for this position
            bool hasVoted = _context.Votes
                .Any(v => v.VoterUsername == username && v.Position == candidate.Position);

            if (hasVoted)
            {
                TempData["Message"] = $"You have already voted for {candidate.Position}!";
                return RedirectToAction("Vote");
            }

            // Increment vote count
            candidate.Votes += 1;
            _context.Votes.Add(new Vote { CandidateId = candidateId, VoterUsername = username, Position = candidate.Position });
            _context.SaveChanges();

            TempData["Message"] = $"Vote for {candidate.Name} ({candidate.Position}) successfully cast!";
            return RedirectToAction("Vote");
        }





        // ✅ Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ✅ Voter dashboard (not implemented fully here)
        public IActionResult VoterDashboard()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login");

            return RedirectToAction("Vote"); // immediately see candidates
        }

    }
}
