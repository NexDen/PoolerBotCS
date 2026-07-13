using PoolerBotCS.Models;

namespace PoolerBotCS.Services;

public class BanchoLobbyService : IBanchoLobbyService
{
    
    private readonly PoolerDbContext  _dbContext;
    private readonly IBanchoPlayerService _playerService;
    private readonly IIrcBotService _ircBotService;
    public BanchoLobbyService(PoolerDbContext dbContext, IBanchoPlayerService playerService,  IIrcBotService ircBotService)
    {
        _dbContext = dbContext;
        _playerService = playerService;
        _ircBotService = ircBotService;
    }

    public void Create(string lobbyId)
    {
        if (lobbyId == "#osu") return;
        Log($"Creating lobby {lobbyId}");

        var newLobby = new BanchoLobby()
        {
            MatchId = lobbyId,
            IsActive = true,
            Players = [],
            CurrentMapId = "",
        };

        _dbContext.BanchoLobbies.Add(newLobby);
        
        _dbContext.SaveChanges();
        
        InvitePlayer(lobbyId, "Metro_Turizm");
        Thread.Sleep(5000);
        ChangeMap(lobbyId,"5389812");
    }

    public void ChangeMap(string lobbyId, string mapId, params string[] mods)
    {
        var lobby = FindLobby(lobbyId);
        if (lobby == null) return;
        
        _ircBotService.SendMessage(lobbyId, $"!mp map {mapId} {string.Join(" ", mods)}");
        lobby.CurrentMapId = mapId;
        _dbContext.SaveChanges();
    }

    public void PlayerJoinLobby(string lobbyId, string playerName)
    {
        if (!lobbyId.StartsWith("#mp_")) return;
        var lobby = FindLobby(lobbyId);
        var player = _playerService.GetOrCreatePlayerByName(playerName);
        Log($"Player {player.Username} joined the lobby {lobby?.Id}");
    }


    public void InvitePlayer(string lobbyId, string playerName)
    {
        var lobby = FindLobby(lobbyId);
        if (lobby == null) return;
        _ircBotService.SendMessage(lobbyId, $"!mp invite {SanitizeUsername(playerName)}");
    }
    
    

    private BanchoLobby? FindLobby(string lobbyId)
    {
        var lobby = _dbContext.BanchoLobbies.FirstOrDefault(l => l.MatchId == lobbyId);
        return lobby;
    }
    
    private static void Log(params object?[] parameters)
    {
        var now = DateTime.Now;
        var msgLine = $"{now} // [BANCHOLOBBYSERVICE] ";
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

    private static string SanitizeUsername(string username) => username.Replace(" ", "_");
    
}