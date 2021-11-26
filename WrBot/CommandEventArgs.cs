using System;

public class CommandEventArgs
{
    public bool IsModerator { get; set; }
    public bool IsBroadcaster { get; set;}
    public string Username { get; set; }
    public string Channel { get; set; }
    public string CommandName { get; set; }
    public string[] Parameters { get; private set; }

    public bool HasSetRunner { get; private set; }
    public bool HasSetGame { get; private set; }
    public bool HasSetCategory { get; private set; }
    public string Runner { get; private set; }
    public string Game { get; private set; }
    public string Category { get; private set; }
    public bool HasRunner { get; private set; }
    public bool HasGame { get; private set; }
    public bool HasCategory { get; private set; }
    public bool HasReset { get; private set; }
    
    public CommandEventArgs(string commandName, string[] parameters)
    {
        this.CommandName = commandName;
        this.Parameters = parameters;

        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].ToLower() == "-setrunner" && parameters.Length > i)
            {
                this.HasSetRunner = true;
                this.Runner = parameters[i + 1];
            }

            if (parameters[i].ToLower() == "-setgame" && parameters.Length > i)
            {
                this.HasSetGame = true;
                this.Game = parameters[i + 1];
            }

            if (parameters[i].ToLower() == "-setcategory" && parameters.Length > i)
            {
                this.HasSetCategory = true;
                this.Category = parameters[i + 1];
            }

            if (parameters[i].ToLower() == "-reset")
            {
                this.HasReset = true;
            }
        }

        if (!this.HasSetRunner && !this.HasSetGame && !this.HasSetCategory && !this.HasReset)
        {
            if (commandName.Equals("wr", StringComparison.InvariantCultureIgnoreCase)) //only game and category parameters
            {
                if (parameters.Length > 0)
                {
                    this.HasGame = true;
                    this.Game = parameters[0];
                }

                if (parameters.Length > 1)
                {
                    this.HasCategory = true;
                    this.Category = parameters[1];
                }
            }
            else if(commandName.Equals("pb", StringComparison.InvariantCultureIgnoreCase))
            {
                if (parameters.Length > 0)
                {
                    this.HasRunner = true;
                    this.Runner = parameters[0];
                }

                if (parameters.Length > 1)
                {
                    this.HasGame = true;
                    this.Game = parameters[1];
                }

                if (parameters.Length > 2)
                {
                    this.HasCategory = true;
                    this.Category = parameters[2];
                }
            }
        }
    }
}