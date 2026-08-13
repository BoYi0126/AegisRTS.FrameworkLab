using UnityEngine;

namespace AegisRTS.Demo.PlayablePrototype
{
    public enum PrototypeExitAction
    {
        StopEditorPlayMode,
        QuitPlayer,
    }

    /// <summary>Keeps application lifecycle calls at the Unity presentation boundary.</summary>
    public static class PrototypeApplicationAdapter
    {
        public static PrototypeExitAction ResolveExitAction(bool isEditor) =>
            isEditor ? PrototypeExitAction.StopEditorPlayMode : PrototypeExitAction.QuitPlayer;

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}
