using System;
using SrcFacade.Models;

namespace WrBotTests.Builders
{
    public class RunBuilder
    {
        public static RunBuilder Init() => new RunBuilder();

        Random Random = new Random();

        string PlayerId = RandomStringBuilder.Init().Build();
        string PlayerName = RandomStringBuilder.Init().Build();
        decimal PrimarySeconds;

        public RunBuilder()
        {
            this.PrimarySeconds = (decimal)(this.Random.Next(1, 100) 
                + this.Random.NextDouble());
        }

        public RunBuilder WithPayerId(string id)
        {
            this.PlayerId = id;
            return this;
        }

        public RunBuilder WithPlayerName(string name)
        {
            this.PlayerName = name;
            return this;
        }

        public RunBuilder WithPrimarySeconds(decimal primarySeconds)
        {
            this.PrimarySeconds = primarySeconds;
            return this;
        }

        public Run Build()
        {
            return new Run
            {
                Players = new Player[] 
                {
                    new Player { Id = this.PlayerId, Name = this.PlayerName }
                },
                Times = new Times
                {
                    PrimarySeconds = this.PrimarySeconds
                }
            };
        }
    }
}