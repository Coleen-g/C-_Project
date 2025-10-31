using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VotingSystem.Models;

namespace VotingSystem.Controllers
{
    public class VoterController : Controller
    {
        private readonly VotingDbContext _context;

        public VoterController(VotingDbContext context)
        {
            _context = context;
        }

        public IActionResult VoterDashboard()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login", "Home");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Home");

            var voter = _context.Voters.FirstOrDefault(v => v.UserId == user.Id);

            if (voter == null)
            {
                voter = new Voter
                {
                    FullName = user.FullName,
                    UserId = user.Id,
                    VoterCode = "#VTR-" + user.Id.ToString("D5"),
                    HasVoted = false
                };
                _context.Voters.Add(voter);
                _context.SaveChanges();
            }

            return View(voter); // 👈 this looks for /Views/Voter/VoterDashboard.cshtml
        }

        public IActionResult Vote()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login", "Home");

            var candidates = _context.Candidates.ToList();

            ViewBag.VotedPositions = _context.Votes
                .Where(v => v.VoterUsername == username)
                .Select(v => v.Position)
                .ToList();

            return View(candidates); // 👈 this looks for /Views/Voter/Vote.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Vote(int candidateId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login", "Home");

            var candidate = _context.Candidates.Find(candidateId);
            if (candidate == null) return NotFound();

            bool hasVoted = _context.Votes.Any(v => v.VoterUsername == username && v.Position == candidate.Position);
            if (hasVoted)
            {
                TempData["Message"] = $"You already voted for {candidate.Position}!";
                return RedirectToAction("Vote");
            }

            candidate.Votes++;
            _context.Votes.Add(new Vote
            {
                CandidateId = candidateId,
                VoterUsername = username,
                Position = candidate.Position
            });
            _context.SaveChanges();

            TempData["Message"] = $"Vote for {candidate.Name} recorded!";
            return RedirectToAction("Vote");
        }

        [AllowAnonymous]
        public IActionResult Results()
        {
            var candidates = _context.Candidates.ToList();
            return View("~/Views/Voter/Results.cshtml", candidates);
        }


    }
}
