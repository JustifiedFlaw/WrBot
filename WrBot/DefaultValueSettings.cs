using System;
using Newtonsoft.Json;

public class DefaultValueSettings
{
    public bool Enabled { get; set; } = false;
    public string Value { get; set; } = null;

    [JsonIgnore]
    public EventHandler<OnSetDefaultChangedArgs> OnSetDefaultChanged;

    public void Set(bool enable, string value)
    {
        var newEnabled = this.Enabled || enable;
        var newValue = enable ? value : this.Value;

        if ((this.Enabled != newEnabled || this.Value != newValue))
        {
            this.Enabled = newEnabled;
            this.Value = newValue;

            this.OnSetDefaultChanged.Invoke(this, new OnSetDefaultChangedArgs());
        }
    }
}