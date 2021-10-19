public class BotSettings
{
    public string BotName { get; set; }
    public string ClientId { get; set; }
    public string AccessToken { get; set; }
    public ChannelSettings[] Channels { get; set; }
}