using System;
using System.Collections.Generic;

public class CommandDictionary : Dictionary<string, Command>
{
    public void Add(string name, Action<CommandEventArgs> action)
    {
        this.Add(name, new Command
        {
            Action = action
        });
    }
}