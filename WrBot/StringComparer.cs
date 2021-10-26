using System;
using System.Text.RegularExpressions;

public static class StringComparer
{
    public static decimal PercentWordMatch(string source, string target)
    {
        var sourceWords = Regex.Split(source.ToLower(), @"\s");
        var targetWords = Regex.Split(target.ToLower(), @"\s");

        decimal percentMatch = 0;
        decimal wordWorth = 100m / (decimal)sourceWords.Length;
        foreach (var sourceWord in sourceWords)
        {
            if (sourceWord.Length > 0)
            {
                var minDistance = sourceWord.Length;

                foreach (var targetWord in targetWords)
                {
                    var distance = LevenshteinDistance(sourceWord, targetWord);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }

                var wordMatch = (decimal)(sourceWord.Length - minDistance) / (decimal)sourceWord.Length;
                percentMatch += wordWorth * wordMatch;
            }
        }

        return percentMatch;
    }

    // Stolen from https://stackoverflow.com/questions/6944056/c-sharp-compare-string-similarity
    public static int LevenshteinDistance(string s, string t)
    {
        if (string.IsNullOrEmpty(s))
        {
            if (string.IsNullOrEmpty(t))
                return 0;
            return t.Length;
        }

        if (string.IsNullOrEmpty(t))
        {
            return s.Length;
        }

        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // initialize the top and right of the table to 0, 1, 2, ...
        for (int i = 0; i <= n; d[i, 0] = i++);
        for (int j = 1; j <= m; d[0, j] = j++);

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                int min1 = d[i - 1, j] + 1;
                int min2 = d[i, j - 1] + 1;
                int min3 = d[i - 1, j - 1] + cost;
                d[i, j] = Math.Min(Math.Min(min1, min2), min3);
            }
        }
        return d[n, m];
    }
}