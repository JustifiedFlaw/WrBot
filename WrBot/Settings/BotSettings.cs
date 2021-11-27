public class BotSettings
{
    public string BotName { get; set; }
    public string ClientId { get; set; }
    public string AccessToken { get; set; }
    public int KeepAlive { get; set; }
    public int KeepChannelsConnected { get; set; }
    public ChannelSettings[] Channels { get; set; }
}