using System.Collections.Generic;
using System.Linq;
using SrcRestEase.Models;

public class SubCategoryAdapter
{
    public static IEnumerable<CategoryVariable> GetSubCategoryCombos(Category category, IEnumerable<Variable> remainingSubCategories)
    {
        if (remainingSubCategories.Count() == 0)
        {
            return new List<CategoryVariable>
            {
                new CategoryVariable
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    SubCategories = new List<SubCategory>()
                }
            };
        }
        else if (remainingSubCategories.Count() == 1)
        {
            var remaining = remainingSubCategories.First();

            return remaining.Values.Values.Select(v => new CategoryVariable
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                SubCategories = new List<SubCategory>
                {
                    new SubCategory
                    {
                        Id = remaining.Id,
                        ValueId = v.Key,
                        ValueName = v.Value.Label
                    }
                }
            });
        }
        else
        {
            var current = remainingSubCategories.First();

            var outputs = new List<CategoryVariable>();
            var combos = GetSubCategoryCombos(category, remainingSubCategories.Skip(1));

            foreach (var currentValue in current.Values.Values)
            {
                foreach (var combo in combos)
                {
                    var subCategory = new CategoryVariable
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        SubCategories = new List<SubCategory>
                        {
                            new SubCategory
                            {
                                Id = current.Id,
                                ValueId = currentValue.Key,
                                ValueName = currentValue.Value.Label
                            }
                        }
                    };

                    subCategory.SubCategories.AddRange(combo.SubCategories);

                    outputs.Add(subCategory);
                }
            }

            return outputs;
        }
    }
}