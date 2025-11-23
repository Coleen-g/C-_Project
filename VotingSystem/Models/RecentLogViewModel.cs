using System;

namespace VotingSystem.Models
{
    public class RecentLogViewModel
    {
        public string VoterUsername { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
