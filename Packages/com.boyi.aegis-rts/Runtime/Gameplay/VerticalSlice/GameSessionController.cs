using System;

namespace AegisRTS.Gameplay.VerticalSlice
{
    public enum GameSessionState { MainMenu, Playing, Paused, Settings, Victory, Defeat }

    public readonly struct GameSettings
    {
        public GameSettings(double masterVolume, double cameraSpeed, bool edgeScrolling)
        {
            if (masterVolume < 0d || masterVolume > 1d || cameraSpeed <= 0d || double.IsNaN(cameraSpeed))
                throw new ArgumentOutOfRangeException(nameof(masterVolume));
            MasterVolume = masterVolume; CameraSpeed = cameraSpeed; EdgeScrolling = edgeScrolling;
        }
        public double MasterVolume { get; }
        public double CameraSpeed { get; }
        public bool EdgeScrolling { get; }
        public static GameSettings Default => new GameSettings(1d, 20d, true);
    }

    public interface IGameSessionBackend
    {
        bool NewGame();
        bool LoadGame();
        bool RestartGame();
    }

    /// <summary>Pure C# menu, pause, settings, outcome, and restart state boundary.</summary>
    public sealed class GameSessionController
    {
        private readonly IGameSessionBackend _backend;
        private GameSessionState _returnFromSettings;
        public GameSessionController(IGameSessionBackend backend)
        { _backend = backend ?? throw new ArgumentNullException(nameof(backend)); State = GameSessionState.MainMenu; Settings = GameSettings.Default; }
        public GameSessionState State { get; private set; }
        public GameSettings Settings { get; private set; }
        public bool NewGame() => State == GameSessionState.MainMenu && Start(_backend.NewGame());
        public bool LoadGame() => State == GameSessionState.MainMenu && Start(_backend.LoadGame());
        public bool Pause() { if (State != GameSessionState.Playing) return false; State = GameSessionState.Paused; return true; }
        public bool Resume() { if (State != GameSessionState.Paused) return false; State = GameSessionState.Playing; return true; }
        public bool OpenSettings()
        { if (State != GameSessionState.MainMenu && State != GameSessionState.Paused) return false; _returnFromSettings = State; State = GameSessionState.Settings; return true; }
        public bool ApplySettings(GameSettings settings)
        { if (State != GameSessionState.Settings) return false; Settings = settings; State = _returnFromSettings; return true; }
        public bool Win() { if (State != GameSessionState.Playing) return false; State = GameSessionState.Victory; return true; }
        public bool Lose() { if (State != GameSessionState.Playing) return false; State = GameSessionState.Defeat; return true; }
        public bool Restart()
        { if (State != GameSessionState.Victory && State != GameSessionState.Defeat) return false; return Start(_backend.RestartGame()); }
        public void ReturnToMenu() => State = GameSessionState.MainMenu;
        private bool Start(bool succeeded) { if (!succeeded) return false; State = GameSessionState.Playing; return true; }
    }
}
