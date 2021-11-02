using System;
using System.Text;

public static class TimeSpanFormater
{
    public static string Format(this TimeSpan timeSpan)
    {
        var format = timeSpan.Milliseconds == 0 && timeSpan.TotalMilliseconds > 0 ?
            "" : @"fff\m\s";

        if (timeSpan.Seconds > 0)
        {
            format = @"ss\s\ " + format;

            if (timeSpan.Minutes > 0)
            {
                format = @"mm\m\ " + format;

                if (timeSpan.Hours > 0)
                {
                    format = @"hh\h\ " + format;

                    if (timeSpan.Days > 0)
                    {
                        format = @"dd\d\ " + format;
                    }
                }
            }
        }

        return timeSpan.ToString(format).Trim();
    }
}