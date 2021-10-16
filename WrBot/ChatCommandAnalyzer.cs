using System.Linq;
using System.Text.RegularExpressions;

public class ChatCommandAnalyzer
{
    public ChatCommands Command { get; private set; }
    public bool HasSetRunner { get; private set; }
    public bool HasSetGame { get; private set; }
    public bool HasSetCategory { get; private set; }
    public string Runner { get; private set; }
    public string Game { get; private set; }
    public string Category { get; private set; }
    public bool HasRunner { get; private set; }
    public bool HasGame { get; private set; }
    public bool HasCategory { get; private set; }

    public void Analyze(string message)
    {
        ResetDefaults();
        
        var commandMatch = Regex.Match(message, @"^!\w+($|\s)");
        if (commandMatch.Success)
        {
            switch (commandMatch.Value.Trim())
            {
                case "!wr":
                    this.Command = ChatCommands.Wr;
                    break;
                case "!pb":
                    this.Command = ChatCommands.Pb;
                    break;
                case "!joinme":
                    this.Command = ChatCommands.JoinMe;
                    break;
                case "!leaveme":
                    this.Command = ChatCommands.LeaveMe;
                    break;
                default:
                    this.Command = ChatCommands.None;
                    break;
            }
        }

        if (this.Command == ChatCommands.None)
        {
            return;
        }

        var parameters = GetParameters(message);

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
        }

        if (!this.HasSetRunner && !this.HasSetGame && ! this.HasSetCategory)
        {
            if (this.Command == ChatCommands.Wr) //only game and category parameters
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
            else if(this.Command == ChatCommands.Pb)
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

    private string[] GetParameters(string message)
    {
        char[] parmChars = message.ToCharArray();
        bool inQuote = false;
        for (int index = 0; index < parmChars.Length; index++)
        {
            if (parmChars[index] == '"')
                inQuote = !inQuote;
            if (!inQuote && parmChars[index] == ' ')
                parmChars[index] = '\n';
        }

        return (new string(parmChars)).Split('\n')
            .Skip(1)
            .Select(s => s.Trim('"'))
            .ToArray();
    }

    private void ResetDefaults()
    {
        this.Command = ChatCommands.None;
        this.HasSetRunner = false;
        this.HasSetGame = false;
        this.HasSetCategory = false;
        this.HasRunner = false;
        this.HasGame = false;
        this.HasCategory = false;
        this.Runner = null;
        this.Game = null;
        this.Category = null;
    }
}