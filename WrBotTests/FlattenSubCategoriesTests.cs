using System.Collections.Generic;
using System.Linq;
using SrcRestEase.Models;
using Xunit;

namespace WrBotTests
{
    public class FlattenSubCategoriesTests
    {
        [Fact]
        public void When_Flattening_SubCategories_Then_Flattened()
        {
            var category = new Category
            {
                Id = "cat",
                Name = "Any%"
            };

            var subCategories = new List<Variable>
            {
                new Variable
                {
                    Id = "gli",
                    Values = new VariableValues
                    {
                        Values = new Dictionary<string, VariableValuesValues>
                        {
                            { 
                                "std", new VariableValuesValues
                                {
                                    Label = "Standard"
                                } 
                            },
                            { 
                                "nmg", new VariableValuesValues
                                {
                                    Label = "No Major Glitches"
                                } 
                            }
                        }
                    }
                },
                new Variable
                {
                    Id = "dif",
                    Values = new VariableValues
                    {
                        Values = new Dictionary<string, VariableValuesValues>
                        {
                            { 
                                "nor", new VariableValuesValues
                                {
                                    Label = "Normal"
                                } 
                            },
                            { 
                                "har", new VariableValuesValues
                                {
                                    Label = "Hard"
                                } 
                            }
                        }
                    }
                },
                new Variable
                {
                    Id = "con",
                    Values = new VariableValues
                    {
                        Values = new Dictionary<string, VariableValuesValues>
                        {
                            { 
                                "pc", new VariableValuesValues
                                {
                                    Label = "PC"
                                } 
                            },
                            { 
                                "swt", new VariableValuesValues
                                {
                                    Label = "Switch"
                                } 
                            }
                        }
                    }
                }
            };

            var flattened = Bot.GetSubCategoryCombos(category, subCategories).ToList();

            Assert.Equal(8, flattened.Count);
            
            var standardNormalPc = flattened[0];
            Assert.Equal("cat", standardNormalPc.CategoryId); 
            Assert.Equal("gli", standardNormalPc.SubCategories[0].Id); 
            Assert.Equal("std", standardNormalPc.SubCategories[0].ValueId); 
            Assert.Equal("dif", standardNormalPc.SubCategories[1].Id); 
            Assert.Equal("nor", standardNormalPc.SubCategories[1].ValueId);
            Assert.Equal("con", standardNormalPc.SubCategories[2].Id); 
            Assert.Equal("pc", standardNormalPc.SubCategories[2].ValueId);
            Assert.Equal("Any% Standard Normal PC", standardNormalPc.FullName);

            var standardNormalSwitch = flattened[1];
            Assert.Equal("cat", standardNormalSwitch.CategoryId); 
            Assert.Equal("gli", standardNormalSwitch.SubCategories[0].Id); 
            Assert.Equal("std", standardNormalSwitch.SubCategories[0].ValueId); 
            Assert.Equal("dif", standardNormalSwitch.SubCategories[1].Id); 
            Assert.Equal("nor", standardNormalSwitch.SubCategories[1].ValueId);
            Assert.Equal("con", standardNormalSwitch.SubCategories[2].Id); 
            Assert.Equal("swt", standardNormalSwitch.SubCategories[2].ValueId);
            Assert.Equal("Any% Standard Normal Switch", standardNormalSwitch.FullName);

            var standardHardPc = flattened[2];
            Assert.Equal("cat", standardHardPc.CategoryId); 
            Assert.Equal("gli", standardHardPc.SubCategories[0].Id); 
            Assert.Equal("std", standardHardPc.SubCategories[0].ValueId); 
            Assert.Equal("dif", standardHardPc.SubCategories[1].Id); 
            Assert.Equal("har", standardHardPc.SubCategories[1].ValueId);
            Assert.Equal("con", standardHardPc.SubCategories[2].Id); 
            Assert.Equal("pc", standardNormalPc.SubCategories[2].ValueId);
            Assert.Equal("Any% Standard Hard PC", standardHardPc.FullName);

            var standardHardSwitch = flattened[3];
            Assert.Equal("cat", standardHardSwitch.CategoryId); 
            Assert.Equal("gli", standardHardSwitch.SubCategories[0].Id); 
            Assert.Equal("std", standardHardSwitch.SubCategories[0].ValueId); 
            Assert.Equal("dif", standardHardSwitch.SubCategories[1].Id); 
            Assert.Equal("har", standardHardSwitch.SubCategories[1].ValueId);
            Assert.Equal("con", standardHardSwitch.SubCategories[2].Id); 
            Assert.Equal("swt", standardHardSwitch.SubCategories[2].ValueId);
            Assert.Equal("Any% Standard Hard Switch", standardHardSwitch.FullName);

            var nmgNormalPc = flattened[4];
            Assert.Equal("cat", nmgNormalPc.CategoryId); 
            Assert.Equal("gli", nmgNormalPc.SubCategories[0].Id); 
            Assert.Equal("nmg", nmgNormalPc.SubCategories[0].ValueId); 
            Assert.Equal("dif", nmgNormalPc.SubCategories[1].Id); 
            Assert.Equal("nor", nmgNormalPc.SubCategories[1].ValueId);
            Assert.Equal("con", nmgNormalPc.SubCategories[2].Id); 
            Assert.Equal("pc", nmgNormalPc.SubCategories[2].ValueId);
            Assert.Equal("Any% No Major Glitches Normal PC", nmgNormalPc.FullName);

            var nmgNormalSwitch = flattened[5];
            Assert.Equal("cat", nmgNormalSwitch.CategoryId); 
            Assert.Equal("gli", nmgNormalSwitch.SubCategories[0].Id); 
            Assert.Equal("nmg", nmgNormalSwitch.SubCategories[0].ValueId); 
            Assert.Equal("dif", nmgNormalSwitch.SubCategories[1].Id); 
            Assert.Equal("nor", nmgNormalSwitch.SubCategories[1].ValueId);
            Assert.Equal("con", nmgNormalSwitch.SubCategories[2].Id); 
            Assert.Equal("swt", nmgNormalSwitch.SubCategories[2].ValueId);
            Assert.Equal("Any% No Major Glitches Normal Switch", nmgNormalSwitch.FullName);

            var nmgHardPc = flattened[6];
            Assert.Equal("cat", nmgHardPc.CategoryId); 
            Assert.Equal("gli", nmgHardPc.SubCategories[0].Id); 
            Assert.Equal("nmg", nmgHardPc.SubCategories[0].ValueId); 
            Assert.Equal("dif", nmgHardPc.SubCategories[1].Id); 
            Assert.Equal("har", nmgHardPc.SubCategories[1].ValueId);
            Assert.Equal("con", nmgHardPc.SubCategories[2].Id); 
            Assert.Equal("pc", nmgHardPc.SubCategories[2].ValueId);
            Assert.Equal("Any% No Major Glitches Hard PC", nmgHardPc.FullName);

            var nmgHardSwitch = flattened[7];
            Assert.Equal("cat", nmgHardSwitch.CategoryId); 
            Assert.Equal("gli", nmgHardSwitch.SubCategories[0].Id); 
            Assert.Equal("nmg", nmgHardSwitch.SubCategories[0].ValueId); 
            Assert.Equal("dif", nmgHardSwitch.SubCategories[1].Id); 
            Assert.Equal("har", nmgHardSwitch.SubCategories[1].ValueId);
            Assert.Equal("con", nmgHardSwitch.SubCategories[2].Id); 
            Assert.Equal("swt", nmgHardSwitch.SubCategories[2].ValueId);
            Assert.Equal("Any% No Major Glitches Hard Switch", nmgHardSwitch.FullName);
        }
    }
}