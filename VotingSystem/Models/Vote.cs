public class Vote
{
    public int Id { get; set; }
    public string VoterUsername { get; set; } // username of voter
    public int CandidateId { get; set; }
    public string Position { get; set; } // track position voted
}
