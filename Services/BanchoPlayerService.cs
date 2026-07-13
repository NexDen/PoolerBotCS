using PoolerBotCS.Models;

namespace PoolerBotCS.Services;

public class BanchoPlayerService : IBanchoPlayerService
{
    private readonly PoolerDbContext _dbContext;
    
    public BanchoPlayerService(PoolerDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public BanchoPlayer GetOrCreatePlayerByName(string playerName)
    {
        var existing = _dbContext.BanchoPlayers.FirstOrDefault(bp => bp.Username == playerName);

        if (existing == null)
        {
            var newPlayer = new BanchoPlayer()
            {
                Id = Guid.NewGuid(),
                Username = playerName,
            };
            
            _dbContext.BanchoPlayers.Add(newPlayer);
            _dbContext.SaveChanges();
            
            Log($"Created new player {newPlayer.Username} with id {newPlayer.Id}");
            
            return newPlayer;
        }
        
        return existing;
        
    }
    
    private static void Log(params object?[] parameters)
    {
        var now = DateTime.Now;
        var msgLine = $"{now} // [BANCHOPLAYERSERVICE] ";
        foreach (var param in parameters)
        {
            if (param is object[] items)
            {
                msgLine += "[" + string.Join(", ", items) + "]";
            }
            else
            {
                msgLine += param + " ";
            }
        }

        Console.WriteLine(msgLine);
    }
    
}