using System.Timers;
using Serilog;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;
using TwitchLib.Communication.Events;

public class TwitchBot
{
    public BotSettings Settings { get; set; }
    public CommandDictionary Commands { get; set; } = new CommandDictionary();
    public ITwitchClient TwitchClient { get; set; }

    private Timer KeepTwitchConnectionAlive { get; set; }
    
    public TwitchBot(BotSettings settings, ITwitchClient twitchClient)
    {
        this.Settings = settings;
        this.TwitchClient = twitchClient;

        this.TwitchClient.OnChatCommandReceived += TwitchClient_OnChatCommandReceived;
        this.TwitchClient.OnLog += TwitchClient_OnLog;
        this.TwitchClient.OnDisconnected += TwitchClient_OnDisconnected;
        
        this.KeepTwitchConnectionAlive = new Timer(this.Settings.KeepAlive);
        this.KeepTwitchConnectionAlive.Elapsed += KeepTwitchConnectionAlive_Elapsed;
        this.KeepTwitchConnectionAlive.Start();
    }

    private void KeepTwitchConnectionAlive_Elapsed(object sender, ElapsedEventArgs e)
    {
        this.TwitchClient.SendRaw("PING");
    }

    private void TwitchClient_OnDisconnected(object sender, OnDisconnectedEventArgs e)
    {
        if (!this.TwitchClient.IsConnected)
        {
            Log.Information("Twitch client disconnected. Attempting to reconnect...");

            while (!this.TwitchClient.IsConnected)
            {
                this.TwitchClient.Connect();
                System.Threading.Thread.Sleep(5000);
            }

            foreach (var channelSettings in this.Settings.Channels)
            {
                this.TwitchClient.JoinChannel(channelSettings.Name);
            }
        }
    }

    private void TwitchClient_OnLog(object sender, OnLogArgs e)
    {
        if (e.Data.Contains(" NOTICE "))
        {
            Log.Warning(e.Data);
        }
        else if (e.Data.Contains(" PRIVMSG "))
        {
            Log.Verbose(e.Data);
        }
        else if(!e.Data.Contains(" PART ")
            && !e.Data.Contains(" JOIN ")
            && !e.Data.Contains(" USERSTATE "))
        {
            Log.Debug(e.Data);
        }
    }

    private void TwitchClient_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
    {
        if (this.Commands.TryGetValue(e.Command.CommandText, out var command))
        {
            var commandEventArgs = new CommandEventArgs(e.Command.CommandText, e.Command.ArgumentsAsList.ToArray())
            {
                Channel = e.Command.ChatMessage.Channel,
                Username = e.Command.ChatMessage.Username,
                IsBroadcaster = e.Command.ChatMessage.IsBroadcaster,
                IsModerator = e.Command.ChatMessage.IsModerator
            };
            command.Action.Invoke(commandEventArgs);
        }
    }

    protected void SendMessage(string channel, string message)
    {
        var truncated = Truncate(message, 500);
        Log.Information($"Replied: #{channel} {truncated}");
        this.TwitchClient.SendMessage(channel, truncated);
    }

    private string Truncate(string message, int maxLength)
    {
        if (message.Length > maxLength)
        {
            return message.Substring(0, maxLength - 3) + "...";   
        }
        
        return message;
    }
}