namespace WrBotTests.Builders
{
    public class BotSettingsBuilder
    {
        public static BotSettingsBuilder Init() => new BotSettingsBuilder();

        private string Channel = RandomStringBuilder.Init().Build();

        public BotSettingsBuilder WithChannel(string channelName)
        {
            this.Channel = channelName;
            return this;
        }

        public BotSettings Build()
        {
            return new BotSettings
            {
                BotName = "Bot",
                ClientId = "",
                AccessToken = "",
                KeepAlive = 3000,
                Channels = new ChannelSettings[]
                {
                    new ChannelSettings
                    {
                        Name = this.Channel,
                        Runner = new DefaultValueSettings(this.Channel),
                        Game = new DefaultValueSettings(this.Channel),
                        Category = new DefaultValueSettings(this.Channel)
                    }
                }
            };
        }
    }
}