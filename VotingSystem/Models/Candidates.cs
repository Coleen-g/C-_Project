namespace VotingSystem.Models
{
    public class Candidates
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Party { get; set; }
        public string Position { get; set; } // e.g., President, VP
        public int Votes { get; set; } = 0;
    }

}
