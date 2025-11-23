using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace VotingSystem.Models
{
    public class Candidates
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Party { get; set; }

        [Required]
        public string Position { get; set; }

        public int Votes { get; set; } = 0;

        public string ImagePath { get; set; } // This is stored in DB

        [NotMapped] // This tells EF not to store this property in DB
        public IFormFile ImageFile { get; set; } // For binding uploaded file
    }
}
