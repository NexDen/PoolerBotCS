namespace PoolerBotCS.Models;

public class BanchoPlayer
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string CurrentMatchId { get; set; }
    public int Slot { get; set; }
    public bool IsHost  { get; set; }
    public bool IsReady { get; set; }
}