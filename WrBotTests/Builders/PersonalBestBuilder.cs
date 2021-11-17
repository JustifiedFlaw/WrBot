using System;
using SrcFacade.Models;

namespace WrBotTests.Builders
{
    public class PersonalBestBuilder
    {
        public static PersonalBestBuilder Init() => new PersonalBestBuilder();

        Random Random = new Random();

        int Place;
        string CategoryId = RandomStringBuilder.Init().Build();
        decimal PrimarySeconds;

        public PersonalBestBuilder()
        {
            this.Place = this.Random.Next(1, 300);

            this.PrimarySeconds = (decimal)(this.Random.Next(1, 100) 
                + this.Random.NextDouble());
        }

        public PersonalBestBuilder WithPlace(int place)
        {
            this.Place = place;
            return this;
        }

        public PersonalBestBuilder WithCategoryId(string categoryId)
        {
            this.CategoryId = categoryId;
            return this;
        }

        public PersonalBestBuilder WithPrimarySeconds(decimal primarySeconds)
        {
            this.PrimarySeconds = primarySeconds;
            return this;
        }

        public PersonalBest Build()
        {
            return new PersonalBest
            {
                Place = this.Place,
                Run = new PersonalBestRun
                {
                    CategoryId = this.CategoryId,
                    Times = new Times
                    {
                        PrimarySeconds = this.PrimarySeconds
                    }
                }
            };
        }
    }
}