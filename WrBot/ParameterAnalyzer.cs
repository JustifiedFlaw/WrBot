using System.Linq;

public static class ParameterAnalyzer
{
    public static string[] GetParameters(string message)
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
}