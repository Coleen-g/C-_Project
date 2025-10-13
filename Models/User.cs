using System.ComponentModel.DataAnnotations;

namespace YourProjectName.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Role can be "Admin" or "Voter"
        [StringLength(20)]
        public string Role { get; set; }

        // To track if user has already voted
        public bool HasVoted { get; set; } = false;
    }
}
