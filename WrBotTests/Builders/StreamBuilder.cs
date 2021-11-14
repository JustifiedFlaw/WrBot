using TwitchRestEase.Models;

namespace WrBotTests.Builders
{
    public class StreamBuilder
    {
        public static StreamBuilder Init() => new StreamBuilder();

        string GameName = RandomStringBuilder.Init().Build();
        string Title = RandomStringBuilder.Init().Build();

        public StreamBuilder WithGameName(string gameName)
        {
            this.GameName = gameName;
            return this;
        }

        public StreamBuilder WithTitle(string title)
        {
            this.Title = title;
            return this;
        }

        public Stream Build()
        {
            return new Stream
            {
                GameName = this.GameName,
                Title = this.Title
            };
        }
    }
}