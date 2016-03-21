using UnityEngine;

public static class Timing
{
    public static string GetTimeString(float time)
    {
        string prefix = "";
        if (time < 0)
        {
            prefix = "-";
        }
        time = Mathf.Abs(time);
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        int milliseconds = (int)(Mathf.Round((time % 1) * 1000f));

        return prefix + minutes.ToString("D2") + ":" + seconds.ToString("D2") + "." + milliseconds.ToString("D3");
    }
}
