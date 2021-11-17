using System.Collections.Generic;
using SrcFacade.Models;

namespace WrBotTests.Builders
{
    public class VariableBuilder
    {
        public static VariableBuilder Init() => new VariableBuilder();

        string Id = RandomStringBuilder.Init().Build();
        int ValueCount = 1;

        public VariableBuilder WithId(string id)
        {
            this.Id = id;
            return this;
        }

        public VariableBuilder WithValueCount(int valueCount)
        {
            this.ValueCount = valueCount;
            return this;
        }

        public Variable Build()
        {
            var values = new Dictionary<string, VariableValuesValues>(this.ValueCount);
            for (int i = 0; i < this.ValueCount; i++)
            {
                values.Add(RandomStringBuilder.Init().Build(), new VariableValuesValues 
                {
                    Label = RandomStringBuilder.Init().Build()
                });
            }

            return new Variable
            {
                Id = this.Id,
                IsSubCategory = true,
                Values = new VariableValues
                {
                    Values = values
                }
            };
        }
    }
}