using System;
using System.Text.RegularExpressions;

public class ConsoleAnalyzer
{
    public ConsoleCommands Command { get; private set; }

    public bool HasChannel { get; set; }
    public string Channel { get; private set; }

    public void Analyze(string line)
    {
        ResetDefaults();
        
        if (line  == null)
        {
            return;
        }

        var commandMatch = Regex.Match(line, @"^\w+($|\s)");
        if (commandMatch.Success 
            && Enum.TryParse<ConsoleCommands>(commandMatch.Value, true, out var command))
        {
            this.Command = command;

            var parameters = ParameterAnalyzer.GetParameters(line);

            if (this.Command == ConsoleCommands.Join || this.Command == ConsoleCommands.Leave)
            {
                if (parameters.Length > 0)
                {
                    HasChannel = true;
                    Channel = parameters[0];
                }
            }
        }
        else
        {
            this.Command = ConsoleCommands.None;
        }
    }

    private void ResetDefaults()
    {
        this.Command = ConsoleCommands.None;

        this.HasChannel = false;
        this.Channel = null;
    }
}