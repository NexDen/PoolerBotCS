namespace PoolerBotCS.Services;

public interface IBanchoLobbyService
{
    public void Make(string name);
    public void Create(string lobbyId);
    public void PlayerJoinLobby(string lobbyId, string playerName);

}