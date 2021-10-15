using System.Linq;
using System.Text.RegularExpressions;

public class ChatCommandAnalyzer
{
    public bool IsWr { get; private set; }
    public bool IsPb { get; private set; }
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
        
        this.IsWr = Regex.IsMatch(message, @"^!wr($|\s)"); // starts with !wr and a whitespace
        this.IsPb = Regex.IsMatch(message, @"^!pb($|\s)"); // starts with !pb and a whitespace

        if (!this.IsWr && !this.IsPb)
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
            if (this.IsWr) //only game and category parameters
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
            else //pb: runner, game and category are possible
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