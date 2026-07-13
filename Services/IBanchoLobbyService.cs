namespace PoolerBotCS.Services;

public interface IBanchoLobbyService
{
    public void Create(string lobbyId);
    public void PlayerJoinLobby(string lobbyId, string playerName);

}