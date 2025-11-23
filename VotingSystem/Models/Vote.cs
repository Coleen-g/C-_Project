using System;
using System.ComponentModel.DataAnnotations;

namespace VotingSystem.Models
{
    public class Vote
    {
        [Key]
        public int Id { get; set; }

        public int CandidateId { get; set; }
        public string VoterUsername { get; set; }
        public string Position { get; set; }

       
    }
}
