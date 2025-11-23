using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.IO;
using VotingSystem.Models;
using BCrypt.Net;

namespace VotingSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly VotingDbContext _context;
        private readonly string _imageFolderPath;

        public AdminController(VotingDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _imageFolderPath = Path.Combine(env.WebRootPath, "images");

            if (!Directory.Exists(_imageFolderPath))
                Directory.CreateDirectory(_imageFolderPath);
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return !string.IsNullOrEmpty(role) && role == "Admin";
        }

        // =============================
        // DASHBOARD
        // =============================
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

        // =============================
        // ADD CANDIDATE (GET)
        // =============================
        [HttpGet]
        public IActionResult AddCandidate()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            ViewBag.Positions = _context.Positions.Select(p => p.Name).ToList();
            return View("~/Views/Admin/AddCandidate.cshtml");
        }

        // =============================
        // ADD CANDIDATE (POST + IMAGE UPLOAD)
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCandidate(Candidates candidate, IFormFile ImageFile, string NewPosition)
        {
            if (!IsAdmin())
                return Json(new { success = false, error = "Unauthorized" });

            // Use new position if provided
            if (!string.IsNullOrEmpty(NewPosition))
            {
                candidate.Position = NewPosition;
            }

            // Save image
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(_imageFolderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                candidate.ImagePath = "/images/" + fileName;
            }

            _context.Candidates.Add(candidate);
            _context.SaveChanges();

            return Json(new { success = true, newPosition = NewPosition });
        }


        // =============================
        // VIEW CANDIDATES
        // =============================
        public IActionResult Candidates()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            var candidates = _context.Candidates
                .OrderBy(c => c.Position)
                .ToList();

            // Pass all unique positions to ViewBag
            ViewBag.Positions = _context.Positions
                .Select(p => p.Name)
                .ToList();

            return View(candidates);
        }


        // =============================
        // EDIT CANDIDATE (GET)
        // =============================
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

        // =============================
        // EDIT CANDIDATE (POST + IMAGE REPLACE)
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCandidate(Candidates candidate, IFormFile ImageFile)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            var existingCandidate = _context.Candidates.FirstOrDefault(c => c.Id == candidate.Id);
            if (existingCandidate == null)
                return NotFound();

            // Update basic fields
            existingCandidate.Name = candidate.Name;
            existingCandidate.Party = candidate.Party;
            existingCandidate.Position = candidate.Position;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingCandidate.ImagePath))
                {
                    string oldImage = Path.Combine(_imageFolderPath, Path.GetFileName(existingCandidate.ImagePath));
                    if (System.IO.File.Exists(oldImage))
                        System.IO.File.Delete(oldImage);
                }

                string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string newPath = Path.Combine(_imageFolderPath, newFileName);

                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                existingCandidate.ImagePath = "/images/" + newFileName;
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Candidate updated successfully!";
            return RedirectToAction("Candidates");
        }

        // =============================
        // DELETE CANDIDATE
        // =============================
        public IActionResult DeleteCandidate(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            var candidate = _context.Candidates.Find(id);
            if (candidate != null)
            {
                if (!string.IsNullOrEmpty(candidate.ImagePath))
                {
                    string imagePath = Path.Combine(_imageFolderPath, Path.GetFileName(candidate.ImagePath));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                _context.Candidates.Remove(candidate);
                _context.SaveChanges();
            }

            TempData["SuccessMessage"] = "Candidate deleted successfully!";
            return RedirectToAction("Candidates");
        }

        // =============================
        // RESULTS
        // =============================
        public IActionResult Results()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            return View("~/Views/Admin/Results.cshtml", _context.Candidates.ToList());
        }

        // =============================
        // USER LOGS
        // =============================
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

        // =============================
        // MANAGE VOTERS
        // =============================
        public IActionResult ManageVoters()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            var voters = _context.Users
                .Where(u => u.Role == "Voter")
                .OrderBy(u => u.FullName)
                .ToList();

            return View("~/Views/Admin/ManageVoters.cshtml", voters);
        }

        [HttpGet]
        public IActionResult AddVoter()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddVoter(User voter)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            if (string.IsNullOrWhiteSpace(voter.Username) ||
                string.IsNullOrWhiteSpace(voter.FullName) ||
                string.IsNullOrWhiteSpace(voter.Password))
            {
                ModelState.AddModelError("", "All fields are required");
                return View(voter);
            }

            voter.Role = "Voter";
            voter.Password = BCrypt.Net.BCrypt.HashPassword(voter.Password);

            _context.Users.Add(voter);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Voter added successfully!";
            return RedirectToAction("ManageVoters");
        }

        // EDIT VOTER
        [HttpGet]
        public IActionResult EditVoter(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            var voter = _context.Users.FirstOrDefault(u => u.Id == id && u.Role == "Voter");
            if (voter == null)
                return NotFound();

            return View("~/Views/Admin/EditVoter.cshtml", voter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditVoter(User updatedVoter)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Home");

            ModelState.Remove("Password");

            if (!ModelState.IsValid)
                return View("~/Views/Admin/EditVoter.cshtml", updatedVoter);

            var voter = _context.Users.FirstOrDefault(u => u.Id == updatedVoter.Id);
            if (voter == null)
                return NotFound();

            voter.Username = updatedVoter.Username;
            voter.FullName = updatedVoter.FullName;

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Voter updated!";
            return RedirectToAction("ManageVoters");
        }

        public IActionResult DeleteVoter(int id)
        {
            var voter = _context.Users.FirstOrDefault(u => u.Id == id && u.Role == "Voter");
            if (voter == null)
                return NotFound();

            _context.Users.Remove(voter);
            _context.SaveChanges();

            return RedirectToAction("ManageVoters");
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    }
}
