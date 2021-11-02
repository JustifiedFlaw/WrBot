using System.Collections.Generic;
using System.Linq;

public class CategoryVariable
{
    public string CategoryId { get; internal set; }
    public string CategoryName { get; internal set; }
    public List<SubCategory> SubCategories { get; set; } = new List<SubCategory>();

    public string FullName
    {
        get 
        { 
            if (this.SubCategories.Count == 0)
            {
                return this.CategoryName;
            }
            else
            {
                return this.CategoryName + " " + string.Join(' ', SubCategories.Select(s => s.ValueName));
            }
        }
    }

    public bool HasVariable 
    { 
        get
        {
            return this.SubCategories.Count > 0;
        }
    }
}