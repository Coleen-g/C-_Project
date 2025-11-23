using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotingSystem.Models
{
    public class Voter
    {
        [Key]
        public int VoterId { get; set; }

        [Required]
        public string FullName { get; set; }

        public bool HasVoted { get; set; }

        public string VoterCode { get; set; }

        // 🔹 Foreign key to User
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
