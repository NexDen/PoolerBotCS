namespace PoolerBotCS.Services;

public interface IIrcBotService
{
    public Task SendRawMessage(string message);
    public Task SendMessage(string channel, string message);

}