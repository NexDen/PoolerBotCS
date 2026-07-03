namespace PoolerBotCS.Models;

public class BanchoLobby
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public string MatchId { get; set; }
    public string MatchIrc { get; set; }
    public List<BanchoPlayer> Players { get; set; }
}