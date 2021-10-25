internal class CategoryVariable
{
    public string CategoryId { get; internal set; }
    public string CategoryName { get; internal set; }
    public string VariableId { get; internal set; } = null;
    public string VariableValueId { get; internal set; } = null;
    public string VariableValueName { get; internal set; } = null;

    public string FullName
    {
        get 
        { 
            if (string.IsNullOrEmpty(this.VariableValueName))
            {
                return this.CategoryName;
            }
            else
            {
                return this.CategoryName + " " + this.VariableValueName;
            }
        }
    }

    public bool HasVariable 
    { 
        get
        {
            return !(string.IsNullOrEmpty(this.VariableId)
                || string.IsNullOrEmpty(this.VariableValueId));
        }
    }
}