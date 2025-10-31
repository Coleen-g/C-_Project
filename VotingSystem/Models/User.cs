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

        [Required]
        public string Role { get; set; }

        // ✅ Add this property
        [Required]
        public string FullName { get; set; }

        // 🔹 One-to-one relationship with Voter
        public Voter Voter { get; set; }
    }
}
