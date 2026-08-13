using UnityEngine;

namespace AegisRTS.Demo.PlayablePrototype
{
    /// <summary>Applies the product display policy without coupling gameplay state to Unity display APIs.</summary>
    public static class PrototypeDisplayAdapter
    {
        public static string LastSummary { get; private set; } = "Display mode not configured.";

        public static Vector2Int ResolveNativeSize(int displayWidth, int displayHeight, int fallbackWidth, int fallbackHeight)
        {
            int width = displayWidth > 0 ? displayWidth : Mathf.Max(1, fallbackWidth);
            int height = displayHeight > 0 ? displayHeight : Mathf.Max(1, fallbackHeight);
            return new Vector2Int(width, height);
        }

        public static void ApplyNativeFullscreen()
        {
            if (Application.isEditor)
            {
                LastSummary = $"Unity Editor Game View · {Screen.width}×{Screen.height}";
                return;
            }

            Resolution current = Screen.currentResolution;
            Vector2Int size = ResolveNativeSize(
                Display.main.systemWidth,
                Display.main.systemHeight,
                current.width,
                current.height);
            Screen.SetResolution(size.x, size.y, FullScreenMode.FullScreenWindow);
            LastSummary = $"Native Fullscreen · {size.x}×{size.y}";
            Debug.Log($"[PlayablePrototype Display] {LastSummary}");
        }
    }
}
