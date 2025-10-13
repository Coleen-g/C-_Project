using System.ComponentModel.DataAnnotations;

namespace VotingSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string Role { get; set; } // e.g. "Admin" or "Voter"
    }
}
