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

    public void Make(string name)
    {
        _ircBotService.SendMessage("BanchoBot", $"!mp make {name}");
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
        
        //InvitePlayer(lobbyId, "Metro_Turizm");
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


    public async Task InvitePlayer(string lobbyId, string playerName)
    {
        var lobby = FindLobby(lobbyId);
        if (lobby == null) return;
        var response = await SendCommandAndCollectResponses(lobbyId, $"!mp invite {SanitizeUsername(playerName)}");
    }
    
    

    private async Task<List<string>> SendCommandAndCollectResponses(string lobbyId, string message, TimeSpan? timeout = null)
    {
        var (_, command, parameters) = message.Split();

        var lineCount = 0;

        var contextLobby = FindLobby(lobbyId);
        
        switch (command)
        {
            case "addref" or "removeref":
                lineCount = parameters.Length; // one message per user added/removed
                break;
            case "settings":
                lineCount = 5 + contextLobby?.Players.Count??0; // 5 lines of filler then the player names
                break;
            default:
                lineCount = 1;
                break;
        }

        var result = await _ircBotService.SendMessageAndCollectResponses(lobbyId, message, lineCount, timeout);
        return result;
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