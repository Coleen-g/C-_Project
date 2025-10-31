// File: Models/ViewModels/AdminDashboardViewModel.cs
using System.Collections.Generic;
using VotingSystem.Models;

namespace VotingSystem.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalVoters { get; set; }
        public int TotalVotesCast { get; set; }
        public int TotalPositions { get; set; }
        public double VotingProgress { get; set; } // percentage
        public List<Candidates> Candidates { get; set; }

        // <-- Add this so the view can show recent user logs
        public List<UserLog> RecentLogs { get; set; } = new List<UserLog>();
    }
}
