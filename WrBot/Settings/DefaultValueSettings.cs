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

            InvokeOnSetDefaultChanged();
        }
    }

    internal void Reset()
    {
        this.Enabled = false;
        this.Value = null;
        
        InvokeOnSetDefaultChanged();
    }

    private void InvokeOnSetDefaultChanged()
    {
        if (this.OnSetDefaultChanged != null)
        {
            this.OnSetDefaultChanged.Invoke(this, new OnSetDefaultChangedArgs());
        }
    }
}