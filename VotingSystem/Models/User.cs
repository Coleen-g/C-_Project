using System.ComponentModel.DataAnnotations;

namespace VotingSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        // Username is required
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        // Password is required when creating a voter
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

        // FullName is required
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; }

        // Navigation property
        public Voter Voter { get; set; }
    }
}
