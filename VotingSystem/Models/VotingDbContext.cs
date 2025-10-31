using Microsoft.EntityFrameworkCore;

namespace VotingSystem.Models
{
    public class VotingDbContext : DbContext
    {
        public VotingDbContext(DbContextOptions<VotingDbContext> options) : base(options) { }

        public DbSet<Candidates> Candidates { get; set; }
        public DbSet<User> Users { get; set; } // existing user table (for login)
        public DbSet<Position> Positions { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<UserLog> UserLogs { get; set; }


        // ✅ New table for voter details
        public DbSet<Voter> Voters { get; set; }
    }
}
