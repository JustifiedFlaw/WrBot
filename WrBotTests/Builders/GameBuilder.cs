using SrcRestEase.Models;

namespace WrBotTests.Builders
{
    public class GameBuilder
    {
        public static GameBuilder Init() => new GameBuilder();

        string Id = RandomStringBuilder.Init().Build();
        string Name = RandomStringBuilder.Init().Build();

        public GameBuilder WithId(string id)
        {
            this.Id = id;
            return this;
        }

        public GameBuilder WithName(string name)
        {
            this.Name = name;
            return this;
        }

        public Game Build()
        {
            return new Game
            {
                Id = this.Id,
                Names = new GameNames
                {
                    International = this.Name
                }
            };
        }
    }
}