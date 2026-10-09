using UnityEngine;

/// <summary>
/// Shared helper for drawing styled screen-space prompts via OnGUI.
/// Call PromptUI.Draw() from any MonoBehaviour's OnGUI method.
/// All prompts share the same visual style: dark panel + coloured key badge + text.
/// </summary>
public static class PromptUI
{
    // ── Cached textures (created once) ──────────────────────────────────
    private static Texture2D _bgTex;
    private static Texture2D _badgeTex;
    private static Color     _lastBadgeColor;

    // ── Layout constants ────────────────────────────────────────────────
    private const float BoxW      = 460f;
    private const float BoxH      = 52f;
    private const float BadgeSize = 36f;
    private const float Padding   = 12f;

    /// <summary>
    /// Draw a styled prompt at the bottom-centre of the screen.
    /// </summary>
    /// <param name="keyName">Display name of the key (e.g. "K", "E", "LMB")</param>
    /// <param name="message">Prompt message shown next to the badge</param>
    /// <param name="textColor">Colour of the message text</param>
    /// <param name="badgeColor">Colour of the key badge square</param>
    /// <param name="yOffset">Vertical offset from default position (negative = higher)</param>
    public static void Draw(string keyName, string message, Color textColor, Color badgeColor, float yOffset = 0f)
    {
        // ── Background texture (cached) ──
        if (_bgTex == null)
        {
            _bgTex = new Texture2D(1, 1);
            _bgTex.SetPixel(0, 0, new Color(0.06f, 0.06f, 0.1f, 0.88f));
            _bgTex.Apply();
        }

        // ── Badge texture (re-create if colour changed) ──
        if (_badgeTex == null || _lastBadgeColor != badgeColor)
        {
            if (_badgeTex == null) _badgeTex = new Texture2D(1, 1);
            _badgeTex.SetPixel(0, 0, badgeColor);
            _badgeTex.Apply();
            _lastBadgeColor = badgeColor;
        }

        // ── Positions ──
        float boxX = Screen.width  / 2f - BoxW / 2f;
        float boxY = Screen.height - 120f + yOffset;

        // Background box
        GUIStyle bgStyle = new GUIStyle();
        bgStyle.normal.background = _bgTex;
        GUI.Box(new Rect(boxX, boxY, BoxW, BoxH), "", bgStyle);

        // Key badge
        float badgeX = boxX + Padding;
        float badgeY = boxY + (BoxH - BadgeSize) / 2f;

        GUIStyle badgeBg = new GUIStyle();
        badgeBg.normal.background = _badgeTex;
        GUI.Box(new Rect(badgeX, badgeY, BadgeSize, BadgeSize), "", badgeBg);

        // Key letter inside badge
        GUIStyle keyStyle = new GUIStyle(GUI.skin.label);
        keyStyle.fontSize      = 20;
        keyStyle.fontStyle     = FontStyle.Bold;
        keyStyle.alignment     = TextAnchor.MiddleCenter;
        keyStyle.normal.textColor = new Color(0.08f, 0.08f, 0.12f);
        GUI.Label(new Rect(badgeX, badgeY, BadgeSize, BadgeSize), keyName, keyStyle);

        // Prompt text
        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize      = 20;
        textStyle.fontStyle     = FontStyle.Bold;
        textStyle.alignment     = TextAnchor.MiddleLeft;
        textStyle.normal.textColor = textColor;

        float textX = badgeX + BadgeSize + 14f;
        float textW = BoxW - (textX - boxX) - Padding;
        GUI.Label(new Rect(textX, boxY, textW, BoxH), message, textStyle);
    }

    /// <summary>
    /// Convenience: draw a standard white-text, amber-badge prompt.
    /// </summary>
    public static void Draw(string keyName, string message, float yOffset = 0f)
    {
        Draw(keyName, message, Color.white, new Color(0.95f, 0.65f, 0.15f), yOffset);
    }

    /// <summary>
    /// Returns a clean display name for a KeyCode.
    /// </summary>
    public static string KeyName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.E:      return "E";
            case KeyCode.F:      return "F";
            case KeyCode.K:      return "K";
            case KeyCode.R:      return "R";
            case KeyCode.T:      return "T";
            case KeyCode.G:      return "G";
            case KeyCode.Space:  return "SPACE";
            case KeyCode.Return: return "ENTER";
            case KeyCode.Mouse0: return "LMB";
            case KeyCode.Mouse1: return "RMB";
            default:             return key.ToString().ToUpper();
        }
    }
}
