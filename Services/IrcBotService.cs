
using System.Net.Security;
using System.Net.Sockets;
using PoolerBotCS.Models;

namespace PoolerBotCS.Services;

public class IrcBotService : BackgroundService
{
    private string  _host;
    private int     _port;
    private bool    _ssl;
    private string  _username;
    private string  _password;
    private string  _prefix;
    private string  _baseChannel;

    private TcpClient client;
    private bool _isConnected       = false;
    private bool _isAuthenticated   = false;
    private StreamWriter? _writer;
    
    
    public IrcBotService(IConfiguration config)
    {
        _host        = config["Irc:Host"] ?? "";
        _port        = config.GetValue<int>("Irc:Port");
        _ssl         = config.GetValue<bool>("Irc:Ssl");
        _username    = config["Irc:Username"] ?? "";
        _password    = config["Irc:Password"] ?? "";
        _prefix      = config["Irc:Prefix"] ?? "";
        _baseChannel = "#osu";
        client = new TcpClient();
        
        
    }

    protected override async Task<Task> ExecuteAsync(CancellationToken stoppingToken)
    {
        // connect to osu!irc and initialize read-write streams
        await client.ConnectAsync(_host, _port, stoppingToken);
        
        Stream stream = client.GetStream();
        if (_ssl)
        {
            var sslStream = new SslStream(stream, false);
            await sslStream.AuthenticateAsClientAsync(_host);
            stream = sslStream;
        }
        
        var reader = new StreamReader(stream);
        _writer = new StreamWriter(stream);
        _writer.AutoFlush = true;
        _writer.NewLine = "\r\n";
        
        
        
        // send authentication to osu!irc
        await SendRawMessage($"PASS {_password}");
        await SendRawMessage($"NICK {_username}");
        
        await SendRawMessage($"JOIN {_baseChannel}");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(stoppingToken); // read server messages continuously
            if (line is null) break; // closed connection

            if (line.StartsWith("PING"))
            {
                var payload = line.Substring("PING ".Length);
                await _writer.WriteLineAsync($"PONG {payload}");
                continue;
            }
            
            var (_, command, _) = line.Split(" ");
            // ^^ fucky c# logic

            
            
            LogMessage(line);
            
            switch (command)
            {
                case "376": // RPL_ENDOFMOTD
                    // osu!irc never sends 376 if the authentication is wrong for any reason,
                    // so 376 is a good sign that we are authenticated.
                    // the server normally sends 464 (ERR_PASSWDMISMATCH) if the password 
                    // is wrong, but that message is not guaranteed.
                    _isAuthenticated = true;
                    Log($"Authentication successful!");
                    break;
                
                case "464": // ERR_PASSWDMISMATCH
                    Log("Authentication unsuccessful.");
                    break;
                
                case "001": // RPL_WELCOME 
                    _isConnected = true;
                    Log($"Connected to {_host}:{_port}!");
                    break;
            }
            
            
        }
        return Task.CompletedTask;
    }

    public async Task SendRawMessage(string message)
    {
        if (_writer is null) throw new InvalidOperationException("IRC connection is not established yet.");
        await _writer.WriteLineAsync(message);
    }

    /// <summary>
    /// Sends a message to the specified channel.
    /// </summary>
    /// <param name="channel">The IRC channel to send the message, should be preceded with an #.</param>
    /// <param name="message">The message to be sent.</param>
    /// <exception cref="InvalidOperationException">Raised if the IRC connection has not been established yet.</exception>
    public async Task SendMessage(string channel, string message)
    {
        if (_writer is null) throw new InvalidOperationException("IRC connection is not established yet.");
        await _writer.WriteLineAsync($"PRIVMSG {channel} {message}");
    }
    


    // users prefixed with "+" are connected via external IRC,
    // and users prefixed with "@" are GMTs. "+" really won't
    // affect this program since realistically we won't have 
    // someone playing pooler from IRC, but "@" will be a problem
    // if Zeus decided to play the game.
    // addendum: zeus is not a gmt anymore...
    private static string CleanUsername(string username)
    {
        if (username.StartsWith('+') || username.StartsWith('@'))
        {
            return username[1..];
        }

        return username;
    }
    
    private static void LogMessage(string message, bool ignoreCrap = true)
    {
        var (source, command, parameters) = message.Split(" ");
        
        var (username, _) = source.Split("!"); // 2nd param is always the hostname, can be ignored
        username = CleanUsername(username);
        
        // QUIT sent whenever someone leaves the server (at least 10 per second lmao)
        // 372 = motd, 375 = motd start, we capture motd end
        if (ignoreCrap && 
            command is 
                "QUIT" or "372" or "375" or 
                "JOIN" or "PART" or // a lot per second, really
                "353" or "366"
           ) return;
        if (command is "PRIVMSG")
        {
            Log($"{username}: {string.Join(" ", parameters)}");
        }
        else
        {
            Log(source, command, parameters);
        }
        
    }
    
    
    private static void Log(params object?[] parameters)
    {
       var now = DateTime.Now;
       var msgLine = $"{now} // [IRC] ";
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


public class IRCMessage
{
    public string   Tags        { get; set; }
    public string   Source      { get; set; }
    public string   Command     { get; set; }
    public string[] Parameters  { get; set; }
}

public static class ArrayExtensions
{
    public static void Deconstruct(this string[] array, out string first, out string second)
    {
        first = array.Length > 0 ? array[0] : null;
        second = array.Length > 1 ? array[1] : null;
    }
    public static void Deconstruct(this string[] array, out string first, out string second, out string[] third)
    {
        first = array.Length > 0 ? array[0] : null;
        second = array.Length > 1 ? array[1] : null;
        third = array.Length > 2 ? array[1..] : null;
    }
}