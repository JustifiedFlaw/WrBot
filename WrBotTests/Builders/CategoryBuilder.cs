using SrcFacade.Models;

namespace WrBotTests.Builders
{
    public class CategoryBuilder
    {
        public static CategoryBuilder Init() => new CategoryBuilder();

        string Id = RandomStringBuilder.Init().Build();
        string Name = RandomStringBuilder.Init().Build();
        string Type = "per-game";

        public CategoryBuilder WithId(string id)
        {
            this.Id = id;
            return this;
        }

        public CategoryBuilder WithName(string name)
        {
            this.Name = name;
            return this;
        }

        public CategoryBuilder WithType(string type)
        {
            this.Type = type;
            return this;
        }

        public Category Build()
        {
            return new Category
            {
                Id = this.Id,
                Name = this.Name,
                Type = this.Type
            };
        }
    }
}