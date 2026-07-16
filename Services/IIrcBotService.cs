namespace PoolerBotCS.Services;

public interface IIrcBotService
{
    public Task SendRawMessage(string message);
    public Task SendMessage(string channel, string message);

    /// <summary>
    /// Sends a message to the specified channel, then waits for the next <paramref name="lineCount"/>
    /// PRIVMSG lines sent by BanchoBot in that same channel.
    /// </summary>
    /// <param name="channel">The IRC channel to send the message, should be preceded with an #.</param>
    /// <param name="message">The message to be sent.</param>
    /// <param name="lineCount">How many BanchoBot response lines to wait for.</param>
    /// <param name="timeout">Max time to wait before giving up. Defaults to 10 seconds.</param>
    /// <exception cref="TimeoutException">Raised if BanchoBot did not respond in time.</exception>
    public Task<List<string>> SendMessageAndCollectResponses(string channel, string message, int lineCount, TimeSpan? timeout = null);
}