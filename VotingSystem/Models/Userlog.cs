using System;

namespace VotingSystem.Models
{
    public class UserLog
    {
        public int Id { get; set; }
        public string VoterUsername { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
