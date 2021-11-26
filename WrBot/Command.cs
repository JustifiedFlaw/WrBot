using System;

public class Command
{
    public Action<CommandEventArgs> Action { get; set; }
}