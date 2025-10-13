using Microsoft.EntityFrameworkCore;

namespace VotingSystem.Models
{
    public class VotingDbContext : DbContext
    {
        public VotingDbContext(DbContextOptions<VotingDbContext> options) : base(options) { }

        public DbSet<Candidates> Candidates { get; set; }
        public DbSet<User> Users { get; set; } // existing table
        public DbSet<Position> Positions { get; set; }  // Add this
        public DbSet<Vote> Votes { get; set; }


    }
}
