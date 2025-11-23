using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VotingSystem.Models;
using System.Collections.Generic;

namespace VotingSystem.Controllers
{
    public class VoterController : Controller
    {
        private readonly VotingDbContext _context;

        public VoterController(VotingDbContext context)
        {
            _context = context;
        }

        // Dashboard
        public IActionResult VoterDashboard()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Home");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Home");

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

            return View(voter);
        }

        // GET: Vote page
        public IActionResult Vote()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Home");

            var candidates = _context.Candidates.ToList();

            // Positions user already voted for
            ViewBag.VotedPositions = _context.Votes
                .Where(v => v.VoterUsername == username && v.Position.ToLower() != "senator")
                .Select(v => v.Position)
                .ToList();

            // Senator votes (allow max 2)
            ViewBag.VotedSenators = _context.Votes
                .Where(v => v.VoterUsername == username && v.Position.ToLower() == "senator")
                .Select(v => v.CandidateId)
                .ToList();

            return View(candidates);
        }

        // POST: Submit vote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Vote(int candidateId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Home");

            var candidate = _context.Candidates.Find(candidateId);
            if (candidate == null) return NotFound();

            bool isSenator = candidate.Position.ToLower() == "senator";

            if (isSenator)
            {
                // Check if voter has already voted max 2 senators
                var votedSenators = _context.Votes
                    .Where(v => v.VoterUsername == username && v.Position.ToLower() == "senator")
                    .ToList();

                if (votedSenators.Count >= 2)
                {
                    TempData["Message"] = "You have already voted for 2 senators!";
                    return RedirectToAction("Vote");
                }
            }
            else
            {
                // Check if already voted for this position
                bool hasVotedForPosition = _context.Votes
                    .Any(v => v.VoterUsername == username && v.Position == candidate.Position);

                if (hasVotedForPosition)
                {
                    TempData["Message"] = $"You already voted for {candidate.Position}!";
                    return RedirectToAction("Vote");
                }
            }

            // Record the vote
            candidate.Votes++;
            _context.Votes.Add(new Vote
            {
                CandidateId = candidateId,
                VoterUsername = username,
                Position = candidate.Position
            });

            // Update HasVoted for voter
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                var voter = _context.Voters.FirstOrDefault(v => v.UserId == user.Id);
                if (voter != null)
                {
                    voter.HasVoted = true;
                    _context.Voters.Update(voter);
                }
            }

            _context.SaveChanges();
            TempData["Message"] = $"Vote for {candidate.Name} recorded!";

            return RedirectToAction("Vote");
        }

        // Results page
        [AllowAnonymous]
        public IActionResult Results()
        {
            var candidates = _context.Candidates.ToList();
            return View("~/Views/Voter/Results.cshtml", candidates);
        }

        // AJAX endpoint for dynamic updates
        [HttpGet]
        public IActionResult GetCandidates()
        {
            var candidates = _context.Candidates
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Party,
                    c.Position,
                    c.ImagePath
                })
                .ToList();

            return Json(candidates);
        }
    }
}
