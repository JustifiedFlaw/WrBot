public class SetDefault
{
    public bool Enabled { get; set; } = false;
    public string Value { get; set; } = null;

    public void Set(bool enable, string value)
    {
        this.Enabled = this.Enabled || enable;
        this.Value = enable ? value : this.Value;
    }
}