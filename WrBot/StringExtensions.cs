using System;

public static class StringExtensions
{
    public static bool EqualsIgnoreCase(this string s1, string s2)
    {
        if (s1 == null)
        {
            return s2 == null;
        }

        return s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase);
    }
}