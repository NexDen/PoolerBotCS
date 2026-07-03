namespace PoolerBotCS.Services;

public interface IIrcBotService
{
    protected Task<Task> ExecuteAsync(CancellationToken stoppingToken);
    public Task SendRawMessage(string message);
    public Task SendMessage(string channel, string message);

}