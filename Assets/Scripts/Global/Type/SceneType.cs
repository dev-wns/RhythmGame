
public enum SceneType : int // Build Index
{
    FreeStyle = 1, Game, Result, // SinglePlay 
    Lobby, Stage,                // MultiPlay
};

public enum SceneAction : byte
{
    Main, Exit,                                             // Public
    Search, GameSetting, SystemSetting, KeySetting, Reload, // FreeStyle
    Pause, GameOver,                                        // InGame
    Login, CreateStage,                                     // MultiPlay
}