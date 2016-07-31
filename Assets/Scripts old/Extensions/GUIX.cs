using UnityEngine;

public static class GUIX
{
    private static int nextWindowID = 0;

    public static void ShadowLabel(Rect rect, string text, GUIStyle style, int offset)
    {
        style = new GUIStyle(style);
        style.normal.textColor = Color.black;
        rect = new Rect(rect.x + offset, rect.y + offset, rect.width, rect.height);
        GUI.Label(rect, text, style);
    }

    public static int NewWindow()
    {
        return nextWindowID++;
    }
}
