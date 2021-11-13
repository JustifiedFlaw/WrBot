using System;
using System.Linq;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

public static class TwitchClientInitializer
{
    public static TwitchClient InitializeTwitchClient(BotSettings settings)
    {
        var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
        var customClient = new WebSocketClient(clientOptions);
        var client = new TwitchClient(customClient);

        var credentials = new ConnectionCredentials(settings.BotName, settings.AccessToken);

        client.Initialize(credentials, settings.Channels.Select(c => c.Name).ToList());

        client.Connect();

        foreach (var channelSettings in settings.Channels)
        {
            client.JoinChannel(channelSettings.Name);
        }

        return client;
    }   
}