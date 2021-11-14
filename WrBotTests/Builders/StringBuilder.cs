using System;
using System.Text;

namespace WrBotTests.Builders
{
    public class RandomStringBuilder
    {
        public static RandomStringBuilder Init() => new RandomStringBuilder();

        int Length = 5;
        Random Random = new Random();

        public RandomStringBuilder WithLength(int length)
        {
            this.Length = length;
            return this;
        }

        public string Build()
        {
            StringBuilder sb = new StringBuilder(this.Length);

            for (int i = 0; i < this.Length; i++)
            {
                sb.Append(RandomChar());
            }

            return sb.ToString();
        }

        private char RandomChar()
        {
            const int min = (int)'a';
            const int max = (int)'z';

            int randomNumber = this.Random.Next(min, max + 1);

            return (char)randomNumber;
        }
    }
}