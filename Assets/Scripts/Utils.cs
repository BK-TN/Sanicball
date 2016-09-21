using System;
using System.Collections;

public static class Utils
{
    public static string GetTimeString(TimeSpan timeToUse)
    {
        return string.Format("{0:00}:{1:00}.{2:000}", timeToUse.Minutes, timeToUse.Seconds, timeToUse.Milliseconds);
    }

    public static string GetPosString(int pos)
    {
        if (pos % 10 == 1 && pos % 100 != 11) return pos + "st";
        if (pos % 10 == 2 && pos % 100 != 12) return pos + "nd";
        if (pos % 10 == 3 && pos % 100 != 13) return pos + "rd";
        return pos + "th";
    }
}