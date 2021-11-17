using SrcFacade.Models;

namespace WrBotTests.Builders
{
    public class UserBuilder
    {
        public static UserBuilder Init() => new UserBuilder();

        string Id = RandomStringBuilder.Init().Build();
        string Name = RandomStringBuilder.Init().Build();

        public UserBuilder WithId(string id)
        {
            this.Id = id;
            return this;
        }

        public UserBuilder WithName(string name)
        {
            this.Name = name;
            return this;
        }

        public User Build()
        {
            return new User
            {
                Id = this.Id,
                Names = new UserNames
                {
                    International = this.Name
                }
            };
        }
    }
}